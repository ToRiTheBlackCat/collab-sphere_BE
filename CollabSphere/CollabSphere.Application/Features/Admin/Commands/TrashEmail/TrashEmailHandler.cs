using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Commands.TrashEmail
{
    public class TrashEmailHandler : CommandHandler<TrashEmailCommand>
    {
        private readonly EmailService _emailService;

        public TrashEmailHandler(EmailService emailService)
        {
            _emailService = emailService;
        }

        protected override async Task<CommandResult> HandleCommand(TrashEmailCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                var isDelete = await _emailService.TrashEmailAsync(request.Id);
                if (isDelete)
                {
                    result.IsSuccess = true;
                    result.Message = $"Put to trask email with ID: {request.Id} successfully";
                }
                else
                {
                    result.Message = $"Put to trask email with ID: {request.Id} failed";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, TrashEmailCommand request)
        {
        }
    }
}
