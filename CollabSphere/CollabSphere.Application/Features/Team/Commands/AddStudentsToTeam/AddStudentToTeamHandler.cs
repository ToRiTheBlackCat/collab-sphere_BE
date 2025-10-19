using CloudinaryDotNet.Core;
using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.AddStudentsToTeam
{
    public class AddStudentToTeamHandler : CommandHandler<AddStudentToTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddStudentToTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(AddStudentToTeamCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            int maxAdded = 5;
            int addedCount = 0;
            StringBuilder rawMessage = new StringBuilder();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find all existed members of team
                var existedTeamMems = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(request.TeamId);

                //Max student can be added
                maxAdded -= existedTeamMems.Count;

                foreach (var student in request.StudentList)
                {
                    if (maxAdded > 0)
                    {
                        //Find classmember
                        var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(student.ClassId, student.StudentId);

                        //If not in class
                        if (foundClassMem == null)
                        {
                            rawMessage.Append($"Cannot add student with id: {student.StudentId} to team. This student not in this class with id: {student.ClassId} | ");
                            continue;
                        }
                        //If in class
                        else
                        {
                            //Check if already in any team
                            if (foundClassMem.TeamId != null)
                            {
                                //Same team
                                if (foundClassMem.TeamId == request.TeamId)
                                {
                                    rawMessage.Append($"Cannot add this student because this student with id: {student.StudentId} already in this team. | ");
                                }
                                //Other team
                                else
                                {
                                    rawMessage.Append($"Cannot add this student with id: {student.StudentId} already in other team with id: {foundClassMem.TeamId}. | ");
                                }
                                continue;
                            }
                            //If not in any team
                            else
                            {
                                foundClassMem.TeamId = request.TeamId;
                                foundClassMem.TeamRole = (int)TeamRole.MEMBER;
                                foundClassMem.IsGrouped = true;

                                _unitOfWork.ClassMemberRepo.Update(foundClassMem);
                                await _unitOfWork.SaveChangesAsync();

                                addedCount++;
                                maxAdded--;
                            }
                        }
                    }
                    //Full of member in team
                    else
                    {
                        result.IsSuccess = false;
                        rawMessage.Append($"Reach the max members of team, cannot add anymore. Fail to added student with id: {student.StudentId} into team with id: {request.TeamId}| ");
                    }
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.IsSuccess = false;
                result.Message = "An error occurred while adding student to team";
            }

            await _unitOfWork.CommitTransactionAsync();

            rawMessage.Append($"Add total {addedCount} students into team with id: {request.TeamId}");
            result.Message = rawMessage.ToString();

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AddStudentToTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER };
            if (bypassRoles.Contains(request.UserRole))
            {
                

                //Check team existed
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "TeamId",
                        Message = $"Cannot find team with id {request.TeamId}."
                    });
                    return;
                }

                //Check lecturer of class
                if (request.UserId != foundTeam.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "UserId",
                        Message = $"You are not the lecturer of this class.You do not have permission to add students to team."
                    });
                    return;
                }

                //Check if input list > 0
                if (request.StudentList == null || request.StudentList.Count == 0)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "StudentList",
                        Message = $"Cannot add empty student list into team."
                    });
                    return;
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You do not have permission to add students to team."
                });
                return;
            }
        }
    }
}
