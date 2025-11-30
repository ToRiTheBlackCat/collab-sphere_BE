using CloudinaryDotNet;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.SystemReport.Commands.CreateSystemReport
{
    public class CreateSystemReportHandler : CommandHandler<CreateSystemReportCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly EmailSender _emailSender;
        private string _userEmail = string.Empty;

        public CreateSystemReportHandler(IUnitOfWork unitOfWork,IConfiguration configure)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _emailSender = new EmailSender(_configure);
        }

        protected override async Task<CommandResult> HandleCommand(CreateSystemReportCommand request, CancellationToken cancellationToken)
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

                _emailSender.SendSystemReport(_userEmail, request.Title, request.Content, request.Attachments);
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateSystemReportCommand request)
        {
            //find user
            var foundUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(request.UserId);
            if (foundUser == null)
            {
                errors.Add(new OperationError()
                {
                    Field = "UserId",
                    Message = $"Not found any user with ID: {request.UserId}"
                });
                return;
            }
            _userEmail = foundUser.Email;
        }
    }
}
