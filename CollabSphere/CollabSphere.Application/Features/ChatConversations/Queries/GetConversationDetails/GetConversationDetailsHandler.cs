using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations;
using CollabSphere.Application.Mappings.ChatConversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Queries.GetConversationDetails
{
    public class GetConversationDetailsHandler : QueryHandler<GetConversationDetailsQuery, GetConversationDetailsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetConversationDetailsHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetConversationDetailsResult> HandleCommand(GetConversationDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetConversationDetailsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var chatConversations = await _unitOfWork.ChatConversationRepo.GetConversationDetail(request.ConversationId);

                chatConversations!.Team.Class.Lecturer.AvatarImg = await _cloudinaryService.GetImageUrl(chatConversations!.Team.Class.Lecturer.AvatarImg);
                var convertImageTasks = chatConversations.Team.ClassMembers.Select(async x =>
                {
                    x.Student.AvatarImg = await _cloudinaryService.GetImageUrl(x.Student.AvatarImg);
                    return x;
                }).ToList();
                await Task.WhenAll(convertImageTasks);

                result.ChatConversation = chatConversations!.ToDetailDto();

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetConversationDetailsQuery request)
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

            // Get team to validate requester
            var team = (await _unitOfWork.TeamRepo.GetTeamDetail(chatConversation.TeamId))!;

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
    }
}
