using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries.AdminGetEmails
{
    public class AdminGetEmailsHandler : QueryHandler<AdminGetEmailsQuery, AdminGetEmailsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailService _emailService;

        public AdminGetEmailsHandler(IUnitOfWork unitOfWork, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        protected override async Task<AdminGetEmailsResult> HandleCommand(AdminGetEmailsQuery request, CancellationToken cancellationToken)
        {
            var result = new AdminGetEmailsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                var emailList = await _emailService.GetRecentEmailsAsync(request.Count);

                result.PaginatedEmails = new PagedList<EmailDto>(
                     list: emailList,
                   pageNum: request.PageNum,
                   pageSize: request.PageSize,
                   viewAll: request.ViewAll
                    );
                result.IsSuccess = true;
                result.Message = "Get email successfully";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AdminGetEmailsQuery request)
        {
        }
    }
}
