using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.DeleteApprovedProject
{
    public class DeleteApprovedProjectHandler : CommandHandler<DeleteApprovedProjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteApprovedProjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteApprovedProjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                var project = (await _unitOfWork.ProjectRepo.GetById(request.ProjectId))!;
                project.Status = (int)ProjectStatuses.REMOVED; // Set to soft delete so the owning lecturer can still access
                project.UpdatedAt = DateTime.UtcNow;
                project.UpdatedBy = request.UserId;

                _unitOfWork.ProjectRepo.Update(project);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted project '{project.ProjectName}' ({project.ProjectId}) successfully.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteApprovedProjectCommand request)
        {
            // Check projectId
            var project = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
            if (project == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ProjectId),
                    Message = $"No project with ID: {request.ProjectId}",
                });
                return;
            }

            // Check project's status
            if (project.Status != (int)ProjectStatuses.APPROVED)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ProjectId),
                    Message = $"Can not delete a project that is not {ProjectStatuses.APPROVED.ToString()}.",
                });
                return;
            }

            // Check if is Assigned to class
            var projectAssignments = await _unitOfWork.ProjectAssignmentRepo.GetProjectAssignmentsByProjectAsync(request.ProjectId);
            if (projectAssignments.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ProjectId),
                    Message = $"Can not delete a project that is already assigned to classe(s). ClassId(s): {string.Join(", ", projectAssignments.Select(x => x.ClassId))}",
                });
            }
        }
    }
}
