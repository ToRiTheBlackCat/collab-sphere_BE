using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectAssignments.Commands.AssignProjectsToClass
{
    public class AssignProjectsToClassHandler : CommandHandler<AssignProjectsToClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AssignProjectsToClassHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(AssignProjectsToClassCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                // Get current time
                var assignDate = DateTime.UtcNow;

                // Get existing assigned project
                var projectAssignments = await _unitOfWork.ProjectAssignmentRepo.GetProjectAssignmentsByClassAsync(request.ClassId);
                var assignedProjectIds = projectAssignments.Select(x => x.ProjectId).ToHashSet(); // ProjectIds of assigned projects

                await _unitOfWork.BeginTransactionAsync();

                #region Data Operations
                // Remove all project assignments not in request
                var projectAssinmentsToRemove = projectAssignments.Where(x => !request.ProjectIds.Contains(x.ProjectId));
                foreach (var projectAssignment in projectAssinmentsToRemove)
                {
                    if (!request.ProjectIds.Contains(projectAssignment.ProjectId))
                    {
                        // Remove references to avoid double tracking when deleting
                        projectAssignment.Class = null;
                        projectAssignment.Project = null;

                        _unitOfWork.ProjectAssignmentRepo.Delete(projectAssignment);
                    }
                }
                await _unitOfWork.SaveChangesAsync();

                // Create assignment for new Projects
                var newProjectIds = request.ProjectIds.Where(x => !assignedProjectIds.Contains(x)); // ProjectIds of new projects
                foreach (var projectId in newProjectIds)
                {
                    var newProjectAssign = new ProjectAssignment()
                    {
                        AssignedDate = assignDate,
                        ClassId = request.ClassId,
                        ProjectId = projectId,
                    };

                    await _unitOfWork.ProjectAssignmentRepo.Create(newProjectAssign);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Class with ID '{request.ClassId}'. Assigned {newProjectIds.Count()} project(s). Removed {projectAssinmentsToRemove.Count()} project(s)";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AssignProjectsToClassCommand request)
        {
            // Check ClassId
            var classEntity = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (classEntity == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ClassId),
                    Message = $"No Class with ID: {request.ClassId}",
                });
                return;
            }

            var bypassRoles = new List<int>() { RoleConstants.HEAD_DEPARTMENT }; // Roles that can bypass the check
            // Check if the viewer is the lecturer assigned to this class
            if (!bypassRoles.Contains(request.Role))
            {
                var lecturer = await _unitOfWork.LecturerRepo.GetById(request.UserId);
                if (lecturer == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "LecturerId",
                        Message = $"No Lecturer with ID: {request.UserId}",
                    });
                    return;
                }
                else if (classEntity.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "LecturerId",
                        Message = $"You are not the Lecturer of this Class ({request.ClassId})",
                    });
                    return;
                }
            }

            // Can't remove ProjectAssignments that are assigned to Teams
            var invalidRemoval = classEntity.Teams
                .Where(x => x.ProjectAssignmentId != null && !request.ProjectIds.Contains(x.ProjectAssignment.ProjectId))
                .Select(x => x.ProjectAssignment.ProjectId)
                .ToList();
            if (invalidRemoval.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.ProjectIds)}",
                    Message = $"Can not remove projects that are assigned to teams. Project IDs: {string.Join(", ", invalidRemoval.Select(x => x))}",
                });
                return;
            }

            // Get existing assigned project
            var assignedProjectIds = classEntity.ProjectAssignments.Select(x => x.ProjectId).ToHashSet();
            for (int i = 0; i < request.ProjectIds.Count; i++)
            {
                var projectId = request.ProjectIds.ElementAt(i);
                // Skip check if project is already assigned
                if (assignedProjectIds.Contains(projectId))
                {
                    continue;
                }

                // If assigning new project then check if the project can be assign
                var projectIdprefix = $"{nameof(request.ProjectIds)}[{i}]";

                // If project does exist
                var project = await _unitOfWork.ProjectRepo.GetById(projectId);
                if (project == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = projectIdprefix,
                        Message = $"No Project with ID: {projectId}",
                    });
                }
                // If project is approved
                else if (project.Status != (int)ProjectStatuses.APPROVED)
                {
                    errors.Add(new OperationError()
                    {
                        Field = projectIdprefix,
                        Message = $"Project with ID '{projectId}' is not {ProjectStatuses.APPROVED.ToString()}.",
                    });
                }
            }
        }
    }
}
