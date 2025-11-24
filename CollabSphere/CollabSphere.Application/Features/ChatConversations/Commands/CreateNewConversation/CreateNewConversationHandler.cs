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

namespace CollabSphere.Application.Features.ChatConversations.Commands.CreateNewConversation
{
    public class CreateNewConversationHandler : CommandHandler<CreateNewConversationCommand, CreateNewConversationResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateNewConversationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CreateNewConversationResult> HandleCommand(CreateNewConversationCommand request, CancellationToken cancellationToken)
        {
            var result = new CreateNewConversationResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data operation
                var team = await _unitOfWork.TeamRepo.GetById(request.ChatConversation.TeamId);

                var newConversation = new ChatConversation()
                {
                    ConversationName = request.ChatConversation.ConversationName.Trim(),
                    TeamId = request.ChatConversation.TeamId,
                    CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.ChatConversationRepo.Create(newConversation);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Created chat conversation '{newConversation.ConversationName}' in team '{team!.TeamName}' ({team.TeamId}).";
                result.ConversationId = newConversation.ConversationId;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateNewConversationCommand request)
        {
            var conversationDto = request.ChatConversation;

            // Check if team exist
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(conversationDto.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(conversationDto.TeamId),
                    Message = $"No team with ID '{conversationDto.TeamId}' found."
                });
                return;
            }

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

            // Check for duplated conversation name
            var existingConversation = await _unitOfWork.ChatConversationRepo.GetTeamConversation(conversationDto.TeamId);
            var existingNames = existingConversation.Select(x => x.ConversationName).ToHashSet();
            if (existingNames.Any() && existingNames.Contains(conversationDto.ConversationName.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(conversationDto.ConversationName),
                    Message = $"The team '{team.TeamName}'({team.TeamId}) already have a chat conversation named '{conversationDto.ConversationName}'.",
                });
                return;
            }
        }
    }
}
