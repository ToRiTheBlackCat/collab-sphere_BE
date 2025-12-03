using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations;
using CollabSphere.Domain.Entities;
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
            var chatConversation = await _unitOfWork.ChatConversationRepo.GetById(request.ConversationId);
            if (chatConversation == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ConversationId),
                    Message = $"No conversation with ID '{request.ConversationId}' found."
                });
                return;
            }

            // Only team members (or lecturer) can delete a team conversation
            if (chatConversation.TeamId.HasValue)
            {
                // Get team to validate requester
                var team = (await _unitOfWork.TeamRepo.GetTeamDetail(chatConversation.TeamId.Value))!;

                // Requester is Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    // Check if is class's assigned lecturer
                    if (request.UserId != team.Class.LecturerId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{team.Class.ClassId}'.",
                        });
                    }
                }
                // Requester is Student
                else if (request.UserRole == RoleConstants.STUDENT)
                {
                    // Check if is member of team
                    var isTeamMember = team.ClassMembers.Any(x => x.StudentId == request.UserId);
                    if (!isTeamMember)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You ({request.UserId}) are not a member of the team with ID '{team.TeamId}'.",
                        });
                    }
                }
            }
            // Can not delete a class conversation
            else
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"Conversation '{chatConversation.ConversationName}'({chatConversation.ConversationId}) is a class conversation, which can not be deleted.",
                });
                return;
            }
        }
    }
}
