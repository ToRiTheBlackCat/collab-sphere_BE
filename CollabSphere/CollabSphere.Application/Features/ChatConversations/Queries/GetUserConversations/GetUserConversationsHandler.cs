using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Documents.Queries.GetTeamDocuments;
using CollabSphere.Application.Mappings.ChatConversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations
{
    public class GetUserConversationsHandler : QueryHandler<GetUserConversationsQuery, GetUserConversationsResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserConversationsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetUserConversationsResult> HandleCommand(GetUserConversationsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetUserConversationsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var chatConversations = await _unitOfWork.ChatConversationRepo.GetConversationsByUser(request.UserId, request.SemesterId, request.ClassId);
                result.ChatConversations = chatConversations.ToViewModels(request.UserId);

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetUserConversationsQuery request)
        {
            if (request.SemesterId.HasValue)
            {
                // Check if semester
                var semester = await _unitOfWork.SemesterRepo.GetById(request.SemesterId.Value);
                if (semester == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.ClassId),
                        Message = $"No Semester with ID '{request.SemesterId}' found."
                    });
                    return;
                }
            }

            if (request.ClassId.HasValue)
            {
                // Check if team exist
                var classEntity = await _unitOfWork.ClassRepo.GetClassDetail(request.ClassId.Value);
                if (classEntity == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.ClassId),
                        Message = $"No Class with ID '{request.ClassId}' found."
                    });
                    return;
                }
                
                // Requester is Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    // Check if is class's assigned lecturer
                    if (request.UserId != classEntity.LecturerId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{classEntity.ClassId}'.",
                        });
                        return;
                    }
                }
                // Requester is Student
                else if (request.UserRole == RoleConstants.STUDENT)
                {
                    // Check if is member of team
                    var isTeamMember = classEntity.ClassMembers.Any(x => x.StudentId == request.UserId);
                    if (!isTeamMember)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"You ({request.UserId}) are not a member of the class with ID '{classEntity.ClassName}'({classEntity.ClassId}).",
                        });
                        return;
                    }
                }
            }
        }
    }
}
