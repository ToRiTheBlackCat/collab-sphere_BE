using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Notifications;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Notifications.Queries.GetNotifcationsOfUser
{
    public class GetNotifcationsOfUserHandler : QueryHandler<GetNotifcationsOfUserQuery, GetNotifcationsOfUserResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetNotifcationsOfUserHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetNotifcationsOfUserResult> HandleCommand(GetNotifcationsOfUserQuery request, CancellationToken cancellationToken)
        {
            var result = new GetNotifcationsOfUserResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = "",
            };

            try
            {
                var notifications = await _unitOfWork.NotificationRepo.GetChatNotificationsOfUser(request.UserId);
                notifications = notifications.OrderByDescending(x => x.CreatedAt).ToList();

                result.PaginatedNotifications = new PagedList<NotificationDto>(
                    list: notifications.ToNotificationDtos(userId: request.UserId),
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll
                );
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetNotifcationsOfUserQuery request)
        {
            return;
        }
    }
}
