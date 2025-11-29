using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ObjectiveMilestone;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Project.Commands.CreateProject;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Project.Commands.UpdateProject
{
    public class UpdateProjectHandler : CommandHandler<UpdateProjectCommand>
    {
        private readonly IUnitOfWork _uniUnitOfWork;

        public UpdateProjectHandler(IUnitOfWork uniUnitOfWork)
        {
            _uniUnitOfWork = uniUnitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CreateProjectResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                var projectDto = request.Project;
                var project = (await _uniUnitOfWork.ProjectRepo.GetProjectDetail(projectDto.ProjectId))!;

                await _uniUnitOfWork.BeginTransactionAsync();

                #region Data Operations
                // Update root-level properties
                project.ProjectName = projectDto.ProjectName;
                project.Description = projectDto.Description;
                project.SubjectId = projectDto.SubjectId;
                project.Status = (int)ProjectStatuses.PENDING;
                project.UpdatedAt = DateTime.UtcNow;
                project.UpdatedBy = request.UserId;

                // 1. Remove objectives with ObjectiveId not in request DTO
                var requestObjectiveIds = projectDto.Objectives.Select(x => x.ObjectiveId).ToHashSet();
                var objectivesToDelete = project.Objectives
                    .Where(x => !requestObjectiveIds.Contains(x.ObjectiveId))
                    .ToList();
                foreach (var objective in objectivesToDelete)
                {
                    // Delete all milestones associated with objecitve
                    foreach (var milestone in objective.ObjectiveMilestones)
                    {
                        _uniUnitOfWork.ObjectiveMilestoneRepo.Delete(milestone);
                    }

                    // Delete objective
                    _uniUnitOfWork.ObjectiveRepo.Delete(objective);

                    // Delete reference in project object
                    project.Objectives.Remove(objective);
                }
                await _uniUnitOfWork.SaveChangesAsync();

                // 2. Add or update objectives
                foreach (var objectiveDto in projectDto.Objectives)
                {
                    var objectiveEntity = project.Objectives.FirstOrDefault(obj => obj.ObjectiveId == objectiveDto.ObjectiveId);

                    if (objectiveEntity == null)
                    {
                        // Add new objective
                        var newObjective = new Objective()
                        {
                            Description = objectiveDto.Description,
                            Priority = objectiveDto.Priority,
                            ObjectiveMilestones = objectiveDto.ObjectiveMilestones.Select(x => new ObjectiveMilestone()
                            {
                                Title = x.Title,
                                Description = x.Description,
                                StartDate = x.StartDate,
                                EndDate = x.EndDate,
                            }).ToList(),
                        };
                        project.Objectives.Add(newObjective);
                    }
                    else
                    {
                        // Update existing objective
                        objectiveEntity.Description = objectiveDto.Description;
                        objectiveEntity.Priority = objectiveDto.Priority;

                        // Sync milestones
                        await SyncMilestones(objectiveEntity, objectiveDto.ObjectiveMilestones);
                    }
                }
                await _uniUnitOfWork.SaveChangesAsync();

                _uniUnitOfWork.ProjectRepo.Update(project);
                await _uniUnitOfWork.SaveChangesAsync();
                #endregion

                await _uniUnitOfWork.CommitTransactionAsync();

                result.Message = $"Updated successfully Project with ID '{project.ProjectId}'.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _uniUnitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task SyncMilestones(Objective objective, List<UpdateProjectObjectiveMilestoneDTO> milestoneDtos)
        {
            // Remove milestones not in DTO
            var milestoneIds = milestoneDtos.Select(m => m.ObjectiveMilestoneId).ToHashSet();
            var milestonesToDelete = objective.ObjectiveMilestones
                .Where(m => !milestoneIds.Contains(m.ObjectiveMilestoneId))
                .ToList();
            foreach (var milestoneEntity in milestonesToDelete)
            {
                // Also delete milestone reference in objective
                objective.ObjectiveMilestones.Remove(milestoneEntity);

                // Delete milestone in DB
                _uniUnitOfWork.ObjectiveMilestoneRepo.Delete(milestoneEntity);
            }
            await _uniUnitOfWork.SaveChangesAsync();

            // Add or Update Milestones
            foreach (var msDto in milestoneDtos)
            {
                if (msDto.ObjectiveMilestoneId.HasValue)
                {
                    // Update existing milestone
                    var existMilestone = objective.ObjectiveMilestones.First(m => m.ObjectiveMilestoneId == msDto.ObjectiveMilestoneId);
                    existMilestone.Title = msDto.Title;
                    existMilestone.Description = msDto.Description;
                    existMilestone.StartDate = msDto.StartDate;
                    existMilestone.EndDate = msDto.EndDate;
                }
                else
                {
                    // Create new if no existing ObjectiveMilestone
                    objective.ObjectiveMilestones.Add(new ObjectiveMilestone
                    {
                        Title = msDto.Title,
                        Description = msDto.Description,
                        StartDate = msDto.StartDate,
                        EndDate = msDto.EndDate
                    });
                }
            }
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateProjectCommand request)
        {
            var projectDto = request.Project;

            // Check existing Project
            var project = await _uniUnitOfWork.ProjectRepo.GetProjectDetail(projectDto.ProjectId);
            if (project == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(projectDto.ProjectId),
                    Message = $"No existing Project with this ID: {projectDto.ProjectId}",
                });

                return;
            }

            var validStatuses = new HashSet<ProjectStatuses>() { ProjectStatuses.PENDING, ProjectStatuses.DENIED, ProjectStatuses.REMOVED };

            // Check project status
            if (!validStatuses.Contains((ProjectStatuses)project.Status))
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(projectDto.ProjectId),
                    Message = $"Project with ID '{projectDto.ProjectId}' current status is not: {string.Join(", ", validStatuses.Select(x => x.ToString()))}",
                });

                return;
            }

            // Check is owning Lecturer's LecturerID
            if (request.UserId != project.LecturerId)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.UserId)}",
                    Message = $"UserId ({request.UserId}) doesn't match the Project's LecturerId ({project.LecturerId}).",
                });
            }

            // Check existing Lecturer ID
            var lecturer = await _uniUnitOfWork.LecturerRepo.GetById(request.UserId);
            if (lecturer == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"No existing Lecturer with this ID: {request.UserId}",
                });
            }

            // Check Subject ID
            var subject = await _uniUnitOfWork.SubjectRepo.GetById(projectDto.SubjectId);
            if (subject == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(projectDto.SubjectId),
                    Message = $"No existing Subject with this ID: {projectDto.SubjectId}",
                });
            }

            // Get exising Objects' ObjectiveIds
            var existObjectiveIds = project.Objectives.Select(x => x.ObjectiveId).ToHashSet();

            // Check Objectives
            for (int index = 0; index < request.Project.Objectives.Count; index++)
            {
                var objectiveDto = request.Project.Objectives[index];
                string objectivePrefix = $"{nameof(projectDto.Objectives)}[{index}]";
                HashSet<int>? existMilestoneIds = null;

                // Check existing ObjectiveId
                if (objectiveDto.ObjectiveId.HasValue)
                {
                    if (!existObjectiveIds.Contains(objectiveDto.ObjectiveId.Value))
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"{objectivePrefix}.{nameof(objectiveDto.ObjectiveId)}",
                            Message = $"The Project with ID '{project.ProjectId}' doesn't have any Objective with ID: {objectiveDto.ObjectiveId}",
                        });
                        return;
                    }

                    // Get existing Milestones' ObjectiveMilestoneIds
                    var objectiveEntity = project.Objectives.First(x => x.ObjectiveId == objectiveDto.ObjectiveId);
                    existMilestoneIds = objectiveEntity.ObjectiveMilestones.Select(x => x.ObjectiveMilestoneId).ToHashSet();
                }

                // Check Milestones
                for (int mileIndex = 0; mileIndex < objectiveDto.ObjectiveMilestones.Count; mileIndex++)
                {
                    var milestoneDto = objectiveDto.ObjectiveMilestones[mileIndex];
                    string milestonePrefix = $"{objectivePrefix}.{nameof(objectiveDto.ObjectiveMilestones)}[{mileIndex}]";

                    // Check existing MilestoneId
                    if (milestoneDto.ObjectiveMilestoneId.HasValue && (existMilestoneIds == null || !existMilestoneIds.Contains(milestoneDto.ObjectiveMilestoneId.Value)))
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"{milestonePrefix}.{nameof(milestoneDto.ObjectiveMilestoneId)}",
                            Message = $"The Object with ID '{objectiveDto.ObjectiveId}' doesn't have any Milestone with ID: {milestoneDto.ObjectiveMilestoneId}",
                        });
                        return;
                    }

                    // Check StartDate & EndDate 
                    if (milestoneDto.EndDate < milestoneDto.StartDate)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = $"{milestonePrefix}.{nameof(milestoneDto.EndDate)}",
                            Message = $"Milestone's EndDate must be atleast 2 days after StartDate.",
                        });
                        return;
                    }
                }
            }
        }
    }
}
