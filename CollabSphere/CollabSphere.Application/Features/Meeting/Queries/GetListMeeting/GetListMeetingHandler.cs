using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Queries.GetListMeeting
{
    public class GetListMeetingHandler : QueryHandler<GetListMeetingQuery, GetListMeetingResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetListMeetingHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetListMeetingResult> HandleCommand(GetListMeetingQuery request, CancellationToken cancellationToken)
        {
            var result = new GetListMeetingResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var meetings = await _unitOfWork.MeetingRepo.SearchMeeting(request.TeamId, request.Title, request.ScheduleTime, request.Status, request.IsDesc);
                if (meetings == null || !meetings.Any())
                {
                    result.IsSuccess = true;
                    result.Message = "No meetings found for this team";
                    result.PaginatedMeeting = null;
                    return result;
                }

                result.PaginatedMeeting = new PagedList<Domain.Entities.Meeting>(
                    list: meetings,
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll
                );
                result.IsSuccess = true;
                result.Message = "Get meetings of team successfully";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;   
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetListMeetingQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Find team
            var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
            if (foundTeam == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamId),
                    Message = $"Not found any team with that Id: {request.TeamId}"
                });
                return;
            }

            //Check if role is valid to get meeting
            if (bypassRoles.Contains(request.UserRole))
            {
                //If Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    //Check lecturer of team
                    if (foundTeam.LecturerId != request.UserId)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"You are not the lecturer of this team. Cannot use this function"
                        });
                        return;
                    }
                }
                else
                {
                    //Find stu of team
                    var teamMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.UserId);
                    if (teamMem == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"You are not the student of this team. Cannot use this function"
                        });
                        return;
                    }
                }
            }
            else
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserId),
                    Message = $"This user with ID: {request.UserId} not has permission to get list of meetings of this team."
                });
                return;
            }
        }
    }
}
