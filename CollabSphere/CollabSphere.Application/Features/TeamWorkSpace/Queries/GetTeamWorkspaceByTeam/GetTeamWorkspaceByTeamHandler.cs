using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.TeamWorkspace;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Queries.GetTeamWorkspaceByTeam
{
    public class GetTeamWorkspaceByTeamHandler : QueryHandler<GetTeamWorkspaceByTeamQuery, GetTeamWorkspaceByTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;
        public GetTeamWorkspaceByTeamHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }
        protected override async Task<GetTeamWorkspaceByTeamResult> HandleCommand(GetTeamWorkspaceByTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new GetTeamWorkspaceByTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                //Find Workspace
                var foundWorkspace = await _unitOfWork.TeamWorkspaceRepo.GetOneByTeamId(request.TeamId);
                if (foundWorkspace == null)
                {
                    result.Message = $"Cannot find any workspace with that ID: {request.TeamId}";
                }
                else
                {
                    //Create response DTO
                    var responseDto = new TeamWorkspaceDetailDto
                    {
                        WorkspaceId = foundWorkspace.WorkspaceId,
                        TeamId = foundWorkspace.TeamId,
                        Title = foundWorkspace.Title,
                        CreatedAt = foundWorkspace.CreatedAt
                    };

                    #region Find List of Workspace
                    //Find all List if Woekspace
                    var foundLists = foundWorkspace.Lists;
                    if (foundLists.Count > 0)
                    {
                        var listDtos = new List<ListDto>();

                        foreach (var list in foundLists)
                        {
                            var newListDto = new ListDto
                            {
                                ListId = list.ListId,
                                Position = list.Position,
                                Title = list.Title,
                            };

                            //Add to ListDtos
                            listDtos.Add(newListDto);

                            #region Find Card Of List
                            //Find all Card of List
                            var foundCards = list.Cards;
                            if (foundCards.Count > 0)
                            {
                                var cardDtos = new List<CardDto>();

                                foreach (var card in foundCards)
                                {
                                    var newCardDto = new CardDto
                                    {
                                        CardId = card.CardId,
                                        Title = card.Title,
                                        Description = card.Description,
                                        RiskLevel = card.RiskLevel,
                                        Position = card.Position,
                                        CreatedAt = card.CreatedAt,
                                        DueAt = card.DueAt,
                                        IsComplete = card.IsCompleted,
                                    };

                                    //Add to cardDtos
                                    cardDtos.Add(newCardDto);

                                    #region Find Assignments of Card
                                    //Find all CardAssingment of Card
                                    var foundCardAssignments = card.CardAssignments;
                                    if (foundCardAssignments.Count > 0)
                                    {
                                        var cardAssignmentDtos = new List<CardAssignmentDto>();

                                        foreach (var assign in foundCardAssignments)
                                        {
                                            var newAssignDto = new CardAssignmentDto
                                            {
                                                StudentId = assign.StudentId,
                                                StudentName = assign.StudentName,
                                                AvatarImg = ""
                                            };
                                            newAssignDto.AvatarImg = await _cloudinaryService.GetImageUrl(assign.Avatar);

                                            //Add to cardAssignmentDtos
                                            cardAssignmentDtos.Add(newAssignDto);
                                        }

                                        //Add List CardAssignments to Card
                                        newCardDto.CardAssignmentDtos = cardAssignmentDtos;
                                    }
                                    #endregion

                                    #region Find Task of Card
                                    var foundTasks = card.Tasks;
                                    if (foundTasks.Count > 0)
                                    {
                                        var taskDtos = new List<TaskDto>();

                                        foreach (var task in foundTasks)
                                        {
                                            var newTaskDto = new TaskDto
                                            {
                                                TaskId = task.TaskId,
                                                TaskTitle = task.TaskTitle,
                                                Order = task.Order,
                                            };

                                            //Add to taskDtos
                                            taskDtos.Add(newTaskDto);

                                            #region Find SubTask of Task
                                            var foundSubTasks = task.SubTasks;
                                            if (foundSubTasks.Count > 0)
                                            {
                                                var subTaskDtos = new List<SubTaskDto>();

                                                foreach (var subTask in foundSubTasks)
                                                {
                                                    var newSubTaskDto = new SubTaskDto
                                                    {
                                                        SubTaskId = subTask.SubTaskId,
                                                        SubTaskTitle = subTask.SubTaskTitle,
                                                        Order = subTask.Order,
                                                        IsDone = subTask.IsDone,
                                                    };

                                                    //Add to subTaskDtos
                                                    subTaskDtos.Add(newSubTaskDto);
                                                }

                                                //Add List SubTask to Task
                                                newTaskDto.SubTaskDtos = subTaskDtos;
                                            }
                                            #endregion
                                        }
                                        //Add List Task to Card
                                        newCardDto.TaskDtos = taskDtos;
                                    }
                                    #endregion
                                }
                                //Add List Card to List
                                newListDto.CardDtos = cardDtos;
                            }
                            #endregion                        
                        }
                        //Map List to response DTO
                        responseDto.ListDtos = listDtos;
                    }
                    #endregion

                    result.TeamWorkspaceDetail = responseDto;
                    result.IsSuccess = true;
                    result.Message = $"Get detail of Team Workspace with teamID: {request.TeamId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetTeamWorkspaceByTeamQuery request)
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
            else
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null || foundTeam.Status == 0)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "TeamId",
                        Message = $"Team with the given ID: {request.TeamId} does not exist!"
                    });
                    return;
                }
                else
                {
                    #region If Lecturer
                    if (request.UserRole == RoleConstants.LECTURER)
                    {
                        //Check if lecturer exists
                        var foundLecturer = await _unitOfWork.LecturerRepo.GetById(request.UserId);
                        if (foundLecturer == null)
                        {
                            errors.Add(new OperationError()
                            {
                                Field = "UserId",
                                Message = $"Lecturer with the given ID: {request.UserId} does not exist."
                            });
                        }
                        //Check if lecturer is the owner of the team
                        if (request.UserId != foundTeam.LecturerId)
                            errors.Add(new OperationError()
                            {
                                Field = "UserRole",
                                Message = $"This lecturer with ID: {request.UserId} not has permission to get this team details."
                            });
                    }
                    #endregion
                    #region If Student
                    else if (request.UserRole == RoleConstants.STUDENT)
                    {
                        //Check if student exists
                        var foundStudent = await _unitOfWork.StudentRepo.GetById(request.UserId);
                        if (foundStudent == null)
                        {
                            errors.Add(new OperationError()
                            {
                                Field = "UserId",
                                Message = $"Student with the given ID: {request.UserId} does not exist."
                            });
                        }

                        //Check if student is in the team
                        var studentInClass = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(request.TeamId);

                        if (studentInClass == null || !studentInClass.Any(x => x.StudentId == request.UserId))
                        {
                            errors.Add(new OperationError()
                            {
                                Field = "UserRole",
                                Message = $"This student with ID: {request.UserId} not has permission to get this team details."
                            });
                        }
                    }
                    #endregion
                }
            }
        }
    }
}
