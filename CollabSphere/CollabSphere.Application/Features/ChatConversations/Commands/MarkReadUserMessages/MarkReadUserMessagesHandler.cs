using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations;
using CollabSphere.Domain.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.ChatConversations.Commands.MarkReadUserMessages
{
    public class MarkReadUserMessagesHandler : CommandHandler<MarkReadUserMessagesCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkReadUserMessagesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(MarkReadUserMessagesCommand request, CancellationToken cancellationToken)
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
                int markedCount = 0;
                var currentTime = DateTime.UtcNow;

                var messageRecipients = await _unitOfWork.MessageRecipientRepo
                    .GetMessageRecipientInConversation(request.UserId, request.ConversationId);
                foreach (var recipient in messageRecipients)
                {
                    if (!recipient.IsRead)
                    {
                        recipient.IsRead = true;
                        recipient.ReadAt = currentTime;

                        _unitOfWork.MessageRecipientRepo.Update(recipient);
                        markedCount++;
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Marked read '{markedCount}' message(s).";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, MarkReadUserMessagesCommand request)
        {
            // Check conversation exist
            var chatConversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(request.ConversationId);
            if (chatConversation == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ConversationId),
                    Message = $"No conversation with ID '{request.ConversationId}' found."
                });
                return;
            }

            var isValidUser = chatConversation.Users.Any(x => x.UId == request.UserId);
            if (!isValidUser)
            {
                var conversationUsersStrings = chatConversation.Users.Select(x =>
                {
                    var fullName = x.IsTeacher ? x.Lecturer.Fullname : x.Student.Fullname;
                    var roleString = x.IsTeacher ? " (Lecturer)" : string.Empty;
                    return $"{fullName}({x.UId}){roleString}";
                });

                errors.Add(new OperationError()
                {
                    Field = nameof(request.ConversationId),
                    Message = $"You({request.UserId}) are not a user in this conversation. Valid users are: {string.Join(", ", conversationUsersStrings)}"
                });
                return;
            }
        }
    }
}
