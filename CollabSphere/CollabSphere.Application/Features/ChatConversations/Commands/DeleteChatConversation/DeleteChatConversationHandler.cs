using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.ChatConversations.Commands.CreateNewConversation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.ChatConversations.Commands.DeleteChatConversation
{
    public class DeleteChatConversationHandler : CommandHandler<DeleteChatConversationCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteChatConversationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteChatConversationCommand request, CancellationToken cancellationToken)
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

                #region Data operation
                // Get chat conversation
                var chatConversation = await _unitOfWork.ChatConversationRepo.GetById(request.ConversationId);

                // Get chat messages in chat conversation
                var messages = await _unitOfWork.ChatMessageRepo.GetChatConversationMessages(chatConversation!.ConversationId);
                foreach (var message in messages)
                {
                    // Delete message recipient entities of each message
                    var recipients = await _unitOfWork.MessageRecipientRepo.GetRecipientsOfMessage(message.MessageId);
                    foreach (var recipient in recipients)
                    {
                        _unitOfWork.MessageRecipientRepo.Delete(recipient);
                    }

                    // Delete the message enity
                    _unitOfWork.ChatMessageRepo.Delete(message);
                }
                await _unitOfWork.SaveChangesAsync();

                // Delete the conversation entity
                _unitOfWork.ChatConversationRepo.Delete(chatConversation);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted chat conversation '{chatConversation.ConversationName}'({chatConversation.ConversationId}).";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteChatConversationCommand request)
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

            // Check if user is team member when is team chat
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
                        return;
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
                        return;
                    }
                }
            }
            // Check is class member when is class chat
            else
            {
                var classEntity = (await _unitOfWork.ClassRepo.GetClassDetail(chatConversation.ClassId))!;

                if (request.UserRole == RoleConstants.LECTURER && request.UserId != classEntity.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer for class '{classEntity.ClassName}'({classEntity.ClassId}).",
                    });
                    return;
                }
                else if (request.UserRole == RoleConstants.STUDENT)
                {
                    var classMembers = classEntity.ClassMembers.Select(x => x.StudentId).ToHashSet();
                    if (!classMembers.Contains(request.UserId))
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You ({request.UserId}) are not a member of class '{classEntity.ClassName}'({classEntity.ClassId}).",
                        });
                        return;
                    }
                }
            }
        }
    }
}
