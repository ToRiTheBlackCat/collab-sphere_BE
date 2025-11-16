using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.DeleteSubTask
{
    public class DeleteSubTaskHandler : CommandHandler<DeleteSubTaskCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteSubTaskHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteSubTaskCommand request, CancellationToken cancellationToken)
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
                    //Find tasks of card
                    var tasksOfCard = foundCard.Tasks;
                    if (tasksOfCard.Count > 0)
                    {
                        foreach (var task in tasksOfCard)
                        {
                            //If match with request
                            if (task.TaskId == request.TaskId)
                            {
                                //Find subTasks of task
                                var subTasksOfCard = task.SubTasks;
                                if (subTasksOfCard.Count > 0)
                                {
                                    foreach (var sub in subTasksOfCard)
                                    {
                                        //If match with request
                                        if (sub.SubTaskId == request.SubTaskId)
                                        {
                                            _unitOfWork.SubTaskRepo.Delete(sub);
                                            await _unitOfWork.SaveChangesAsync();
                                            await _unitOfWork.CommitTransactionAsync();
                                            result.IsSuccess = true;

                                            return result;
                                        }
                                    }
                                }
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

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteSubTaskCommand request)
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
                    else
                    {
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
                        else
                        {
                            //Find SubTask
                            var foundSubTask = await _unitOfWork.SubTaskRepo.GetById(request.SubTaskId);
                            if (foundSubTask == null)
                            {
                                errors.Add(new OperationError()
                                {
                                    Field = nameof(request.SubTaskId),
                                    Message = $"Cannot find any subtask with ID: {request.SubTaskId}"
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
    }
}
