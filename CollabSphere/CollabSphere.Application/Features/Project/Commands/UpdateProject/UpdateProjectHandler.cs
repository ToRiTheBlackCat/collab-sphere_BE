using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
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
                project.BusinessRules = projectDto.BusinessRules;
                project.Actors = projectDto.Actors;

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
            var bypassRoles = new List<int>()
            {
                RoleConstants.HEAD_DEPARTMENT,
                RoleConstants.STAFF
            };

            if (!bypassRoles.Contains(request.UserRole))
            {
                // Check Requester's Lecturer ID
                if (request.UserId != project.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(request.UserId)}",
                        Message = $"UserId ({request.UserId}) doesn't match the Project's LecturerId ({project.LecturerId}).",
                    });
                }
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
        }
    }
}
