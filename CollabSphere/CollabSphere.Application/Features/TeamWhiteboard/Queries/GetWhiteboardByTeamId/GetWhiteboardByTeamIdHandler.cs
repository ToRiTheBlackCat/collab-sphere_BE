using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardByTeamId
{
    public class GetWhiteboardByTeamIdHandler : QueryHandler<GetWhiteboardByTeamIdQuery, GetWhiteboardByTeamIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWhiteboardByTeamIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetWhiteboardByTeamIdResult> HandleCommand(GetWhiteboardByTeamIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetWhiteboardByTeamIdResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundTeam = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
                if (foundTeam != null)
                {
                    var foundWhiteboard = await _unitOfWork.TeamWhiteboardRepo.GetByTeamId(request.TeamId);
                    if (foundWhiteboard == null)
                    {
                        //Create new whiteboard
                        var newWhiteboard = new Domain.Entities.TeamWhiteboard
                        {
                            TeamId = foundTeam.TeamId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _unitOfWork.TeamWhiteboardRepo.Create(newWhiteboard);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        //Create default page 
                        var newPage = new WhiteboardPage
                        {
                            WhiteboardId = newWhiteboard.WhiteboardId,
                            PageTitle = "Page1",
                            CreatedAt = DateTime.UtcNow,
                            IsActivate = true,
                        };
                        await _unitOfWork.WhiteboardPageRepo.Create(newPage);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        result.WhiteboardId = newWhiteboard.WhiteboardId;
                    }
                    else
                    {
                        result.WhiteboardId = foundWhiteboard.WhiteboardId;
                    }
                }

                result.IsSuccess = true;
                result.Message = $"Get Whiteboard of team with ID: {request.TeamId} successfully";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetWhiteboardByTeamIdQuery request)
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
                //Check if team exists
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
