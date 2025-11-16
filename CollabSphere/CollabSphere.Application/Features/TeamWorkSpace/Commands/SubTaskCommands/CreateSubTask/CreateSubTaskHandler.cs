using CollabSphere.Application.Base;
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

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.CreateSubTask
{
    public class CreateSubTaskHandler : CommandHandler<CreateSubTaskCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateSubTaskHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateSubTaskCommand request, CancellationToken cancellationToken)
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
                    if (foundCard.Tasks != null && foundCard.Tasks.Count > 0)
                    {
                        foreach (var task in foundCard.Tasks)
                        {
                            //Find match task
                            if (task.TaskId == request.TaskId)
                            {
                                //Get subtask of task
                                var cardSubTasks = task.SubTasks;
                                var subTaskCount = cardSubTasks.Count;

                                //Create new SubTask
                                var newSubTask = new SubTask
                                {
                                    TaskId = task.TaskId,
                                    SubTaskTitle = request.SubTaskTitle,
                                    Order = subTaskCount++,
                                    IsDone = request.IsDone
                                };

                                await _unitOfWork.SubTaskRepo.Create(newSubTask);
                                await _unitOfWork.SaveChangesAsync();
                                await _unitOfWork.CommitTransactionAsync();

                                var createdSubTask = await _unitOfWork.SubTaskRepo.GetById(newSubTask.SubTaskId);

                                var jsonOptions = new JsonSerializerOptions
                                {
                                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                                    WriteIndented = true
                                };
                                result.IsSuccess = true;
                                result.Message = JsonSerializer.Serialize(createdSubTask, jsonOptions);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.IsSuccess = false;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateSubTaskCommand request)
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

                    //Find task
                    var foundTask = await _unitOfWork.TaskRepo.GetById(request.TaskId);
                    if (foundTask == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = nameof(request.TaskId),
                            Message = $"Cannot find any task with ID: {request.TaskId}"
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
