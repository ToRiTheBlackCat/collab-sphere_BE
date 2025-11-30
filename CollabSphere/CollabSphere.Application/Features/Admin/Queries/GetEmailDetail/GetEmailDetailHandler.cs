using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Admin.Queries.AdminGetEmails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries.GetEmailDetail
{
    public class GetEmailDetailHandler : QueryHandler<GetEmailDetailQuery, GetEmailDetailResult>
    {
        private readonly EmailService _emailService;

        public GetEmailDetailHandler( EmailService emailService)
        {
            _emailService = emailService;
        }

        protected override async Task<GetEmailDetailResult> HandleCommand(GetEmailDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new GetEmailDetailResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var emailDetail = await _emailService.GetEmailDetailsAsync(request.Id);
                if (emailDetail != null)
                {
                    result.EmailDetail = emailDetail;
                }
                result.IsSuccess = true;
                result.Message = $"Get Detail of email with ID: {request.Id} successfully";
                
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetEmailDetailQuery request)
        {
        }
    }
}
