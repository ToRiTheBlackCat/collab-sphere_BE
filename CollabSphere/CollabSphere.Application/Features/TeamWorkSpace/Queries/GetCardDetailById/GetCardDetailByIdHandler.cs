using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Queries.GetCardDetailById
{
    public class GetCardDetailByIdHandler : QueryHandler<GetCardDetailByIdQuery, GetCardDetailByIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCardDetailByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetCardDetailByIdResult> HandleCommand(GetCardDetailByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetCardDetailByIdResult
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                CardDetail = null
            };
            try
            {
                var foundCard = await _unitOfWork.CardRepo.GetCardDetailByIdWithAllRelativeInfo(request.CardId);
                if (foundCard != null)
                {
                    result.CardDetail = foundCard;
                }
                result.IsSuccess = true;
                result.Message = $"Get detail of card with ID: {request.CardId} successfully";
            }
            catch (Exception ex)
            {
                result.Message = $"Fail to get detail of card. Error detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetCardDetailByIdQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check role permission
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"This role with ID: {request.UserRole} not has permission to get this team details."
                });
                return;
            }

            //Find team workspace
            var foundWorkspace = await _unitOfWork.TeamWorkspaceRepo.GetById(request.WorkspaceId);
            if (foundWorkspace == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.WorkspaceId),
                    Message = $"Cannot find any workspace with ID: {request.WorkspaceId}"
                });
                return;
            }
            else
            {
                //Find list
                var foundList = await _unitOfWork.ListRepo.GetById(request.ListId);
                if (foundList == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.ListId),
                        Message = $"Cannot find any list with ID: {request.ListId}"
                    });
                    return;
                }
                else
                {
                    //Find user
                    var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);
                    if (foundUser == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.UserId),
                            Message = $"Cannot find any user with ID: {request.UserId}"
                        });
                        return;
                    }
                    else
                    {
                        //Find team
                        var foundTeam = await _unitOfWork.TeamRepo.GetById(foundWorkspace.TeamId);
                        //If Lecturer
                        if (foundUser.IsTeacher)
                        {
                            if (foundTeam?.LecturerId != foundUser.UId)
                            {
                                errors.Add(new OperationError()
                                {
                                    Field = nameof(foundTeam.LecturerId),
                                    Message = $"You are not the lecturer of this team. Cannot use this function"
                                });
                                return;
                            }
                        }
                        //If Student
                        else
                        {
                            var isTeamMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.UserId);
                            if (isTeamMembers == null)
                            {
                                errors.Add(new OperationError()
                                {
                                    Field = nameof(foundTeam.LecturerId),
                                    Message = $"You are not the member of this team. Cannot use this function"
                                });
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
