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

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.CreateCardAndAssignMember
{
    public class CreateCardHandler : CommandHandler<CreateCardCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateCardHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateCardCommand request, CancellationToken cancellationToken)
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

                var foundList = await _unitOfWork.ListRepo.GetById(request.ListId);
                if (foundList != null)
                {
                    //Create new Card
                    var newCard = new Card
                    {
                        ListId = request.ListId,
                        Title = request.CardTitle,
                        Description = request.CardDescription,
                        Position = request.Position,
                        RiskLevel = request.RiskLevel,
                        CreatedAt = request.CreatedAt,
                        DueAt = request.DueAt,
                        IsCompleted = request.IsComplete
                    };

                    await _unitOfWork.CardRepo.Create(newCard);
                    await _unitOfWork.SaveChangesAsync();

                    //Create CardAssignments
                    if (request.AssignmentList.Count > 0)
                    {
                        foreach (var assign in request.AssignmentList)
                        {
                            //Find User
                            var foundStudent = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(assign.StudentId);
                            if (foundStudent != null)
                            {
                                //Create new cardAssignment
                                var newCardAssignment = new CardAssignment
                                {
                                    CardId = newCard.CardId,
                                    StudentId = foundStudent.UId,
                                    StudentName = foundStudent.Student.Fullname,
                                    Avatar = foundStudent.Student.AvatarImg
                                };

                                await _unitOfWork.CardAssignmentRepo.Create(newCardAssignment);
                                await _unitOfWork.SaveChangesAsync();
                            }
                        }
                    }

                    //Create Task
                    if (request.TasksOfCard.Count > 0)
                    {
                        foreach (var task in request.TasksOfCard)
                        {
                            //Create new Task of Card
                            var newTask = new Domain.Entities.Task
                            {
                                CardId = newCard.CardId,
                                Order = task.TaskOrder,
                                TaskTitle = task.TaskTitle,
                            };

                            await _unitOfWork.TaskRepo.Create(newTask);
                            await _unitOfWork.SaveChangesAsync();

                            //Create SubTask
                            if (task.SubTaskOfCard.Count > 0)
                            {
                                foreach (var subTask in task.SubTaskOfCard)
                                {
                                    //Create new SubTask of Task
                                    var newSubTask = new SubTask
                                    {
                                        TaskId = newTask.TaskId,
                                        SubTaskTitle = subTask.SubTaskTitle,
                                        Order = subTask.SubTaskOrder,
                                        IsDone = subTask.IsDone,
                                    };

                                    await _unitOfWork.SubTaskRepo.Create(newSubTask);
                                    await _unitOfWork.SaveChangesAsync();
                                }
                            }
                        }
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    //Find created Card 
                    var createdCard = await _unitOfWork.CardRepo.GetCardDetailByIdWithAllRelativeInfo(newCard.CardId);

                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true 
                    };
                    result.IsSuccess = true;
                    result.Message = JsonSerializer.Serialize(createdCard, jsonOptions);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.IsSuccess = false;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateCardCommand request)
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
