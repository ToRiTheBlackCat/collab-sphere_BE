using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Notifications.Commands.MarkReadNotification
{
    public class MarkReadNotificationHandler : CommandHandler<MarkReadNotificationCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkReadNotificationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(MarkReadNotificationCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = false,
                Message = string.Empty,
                ErrorList = new List<OperationError>(),
            };

            try
            {
                var notification = await _unitOfWork.NotificationRepo.GetNotificationDetail(request.NotificationId);
                if (notification == null)
                {
                    result.ErrorList.Add(new OperationError()
                    {
                        Field = nameof(request.NotificationId),
                        Message = $"No Notification with ID '{request.NotificationId}' found."
                    });
                    return result;
                }

                // Can only mark notification as read if it was sent to the requesting user
                var recipient = notification.NotificationRecipients
                    .SingleOrDefault(x => x.ReceiverId == request.UserId);
                if (recipient == null)
                {
                    result.ErrorList.Add(new OperationError()
                    {
                        Field = nameof(request.NotificationId),
                        Message = $"The Notification with ID '{notification.NotificationId}' was not sent to you."
                    });
                    return result;
                }

                #region Data Operation
                await _unitOfWork.BeginTransactionAsync();

                recipient.IsRead = true;
                recipient.ReadAt = DateTime.UtcNow;

                _unitOfWork.NotificationRecipientRepo.Update(recipient);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                #endregion

                result.Message = $"Marked notification '{notification.Title}'({notification.NotificationId}) as read for you({request.UserId}).";
                result.IsValidInput = true;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, MarkReadNotificationCommand request)
        {
        }
    }
}
