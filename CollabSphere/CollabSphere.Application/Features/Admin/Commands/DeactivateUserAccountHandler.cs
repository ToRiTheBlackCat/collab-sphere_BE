using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Commands
{
    public class DeactivateUserAccountHandler : CommandHandler<DeactivateUserAccountCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeactivateUserAccountHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(DeactivateUserAccountCommand request, CancellationToken cancellationToken)
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

                var foundUser = await _unitOfWork.UserRepo.GetUserAccountIncludeWithAllStatus(request.UserId);
                if (foundUser != null)
                {
                    foundUser.IsActive = !foundUser.IsActive;

                    _unitOfWork.UserRepo.UpdateUser(foundUser);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Activate/Deactivate user with ID: {request.UserId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeactivateUserAccountCommand request)
        {
            //Find existed User
            var foundUser = await _unitOfWork.UserRepo.GetUserAccountIncludeWithAllStatus(request.UserId);
            if (foundUser == null)
            {
                errors.Add(new OperationError()
                {
                    Field = "UserId",
                    Message = $"Cannot find any user with ID: {request.UserId}"
                });
                return;
            }
        }
    }
}
