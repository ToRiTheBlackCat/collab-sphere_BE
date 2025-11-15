using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.CardMemberCommands
{
    public class AssignMembersToCardHandler : CommandHandler<AssignMembersToCardCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public AssignMembersToCardHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<CommandResult> HandleCommand(AssignMembersToCardCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundCard = await _unitOfWork.CardRepo.GetCardDetailByIdWithAllRelativeInfo(request.CardId);
                if (foundCard != null)
                {
                    var foundCardAssignments = foundCard.CardAssignments;

                    //Check if assigned
                    foreach (var assign in foundCardAssignments)
                    {
                        //Already assigned
                        if (assign.StudentId == request.StudentId)
                        {
                            result.IsSuccess = false;
                            return result;
                        }
                    }

                    //Find User
                    var foundStudent = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.StudentId);

                    var avatarImg = await _cloudinaryService.GetImageUrl(foundStudent!.Student.AvatarImg);

                    //Assign more to card
                    var newAssign = new CardAssignment
                    {
                        CardId = request.CardId,
                        StudentId = request.StudentId,
                        StudentName = foundStudent?.Student.Fullname,
                        Avatar = avatarImg
                    };

                    await _unitOfWork.CardAssignmentRepo.Create(newAssign);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var assignedMember = await _unitOfWork.CardAssignmentRepo.GetOneByCardIdAndStuId(foundCard.CardId, foundStudent!.UId);

                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    result.IsSuccess = true;
                    result.Message = JsonSerializer.Serialize(assignedMember, jsonOptions);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.IsSuccess = false;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AssignMembersToCardCommand request)
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
                    //Find card
                    var foundCard = await _unitOfWork.CardRepo.GetById(request.CardId);
                    if (foundCard == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.CardId),
                            Message = $"Cannot find any card with ID: {request.CardId}"
                        });
                        return;
                    }

                    //Find user
                    var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.RequesterId);
                    if (foundUser == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.RequesterId),
                            Message = $"Cannot find any user with ID: {request.RequesterId}"
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
                            var isTeamMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.RequesterId);
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
