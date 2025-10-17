using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.DeleteProject
{
    public class DeleteProjectHandler : CommandHandler<DeleteProjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteProjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                var project = (await _unitOfWork.ProjectRepo.GetById(request.ProjectId))!;
                
                // Remove all Objectives
                foreach (var objective in project.Objectives)
                {
                    // Remove all Milestones
                    foreach (var milestone in objective.ObjectiveMilestones)
                    {
                        _unitOfWork.ObjectiveMilestoneRepo.Delete(milestone);
                    }

                    _unitOfWork.ObjectiveRepo.Delete(objective);
                }
                await _unitOfWork.SaveChangesAsync();

                // Remove project
                _unitOfWork.ProjectRepo.Delete(project);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted project '{project.ProjectName}' ({project.ProjectId}) successfully.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteProjectCommand request)
        {
            // Check ProjectId
            var project = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
            if (project == null)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.ProjectId)}",
                    Message = $"No project with ID: {request.ProjectId}"
                });
                return;
            }

            // Check viewer LecturerId
            if (project.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.ProjectId)}",
                    Message = $"You are not the owning Lecturer of the Project with ID '{project.ProjectId}'."
                });
                return;
            }

            // Check project's status
            if (project.Status == (int)ProjectStatuses.APPROVED)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.ProjectId)}",
                    Message = $"Can not delete a project with {ProjectStatuses.APPROVED.ToString()} status."
                });
                return;
            }
        }
    }
}
