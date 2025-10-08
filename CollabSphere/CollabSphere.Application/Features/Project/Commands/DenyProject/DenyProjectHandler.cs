using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.DenyProject
{
    public class DenyProjectHandler : CommandHandler<DenyProjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DenyProjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        protected override async Task<CommandResult> HandleCommand(DenyProjectCommand request, CancellationToken cancellationToken)
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

                #region Data Operations
                // Get project
                var project = (await _unitOfWork.ProjectRepo.GetById(request.ProjectId))!;
                project.Status = ProjectStatuses.DENIED;

                // Update project
                _unitOfWork.ProjectRepo.Update(project);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Project '{project.ProjectName}' denied.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DenyProjectCommand request)
        {
            var project = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
            if (project != null)
            {
                if (project.Status != ProjectStatuses.PENDING)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.ProjectId),
                        Message = $"Project with ID '{request.ProjectId}' is not Pending",
                    });
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ProjectId),
                    Message = $"No Project with ID: {request.ProjectId}",
                });
            }
        }
    }
}
