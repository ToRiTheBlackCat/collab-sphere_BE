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

                // Get avatar images for lecturer & class members
                var convertImageTasks = chatConversations!.Users.Select(async x =>
                {
                    if (x.IsTeacher)
                    {
                        x.Lecturer.AvatarImg = await _cloudinaryService.GetImageUrl(x.Lecturer.AvatarImg);
                    }
                    else
                    {
                        x.Student.AvatarImg = await _cloudinaryService.GetImageUrl(x.Student.AvatarImg);
                    }
                    return x;
                });
                await Task.WhenAll(convertImageTasks);

                result.ChatConversation = chatConversations.ToDetailDto();
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

            // Check if user is team member when is team conversation
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
            // Check is class member when is class conversation
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
