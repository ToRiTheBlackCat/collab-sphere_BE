using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.WorkSpaceCommands.LeaveWorkspace
{
    public class LeaveWorkspaceHandler : CommandHandler<LeaveWorkspaceCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public LeaveWorkspaceHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(LeaveWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult
            {
                IsSuccess = true,
                IsValidInput = true,
                Message = string.Empty,
            };

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, LeaveWorkspaceCommand request)
        {
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
