using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Meeting.Queries.GetListMeeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Queries.GetMeetingDetail
{
    public class GetMeetingDetailHandler : QueryHandler<GetMeetingDetailQuery, GetMeetingDetailResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetMeetingDetailHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetMeetingDetailResult> HandleCommand(GetMeetingDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new GetMeetingDetailResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var foundMeeting = await _unitOfWork.MeetingRepo.GetById(request.MeetingId);

                result.FoundMeeting = foundMeeting;
                result.IsSuccess = true;
                result.Message = $"Get meeting with ID: {request.MeetingId} successfully";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetMeetingDetailQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Find meeting
            var foundMeeting = await _unitOfWork.MeetingRepo.GetById(request.MeetingId);
            if (foundMeeting == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.MeetingId),
                    Message = $"Not found any meeting with that Id: {request.MeetingId}"
                });
                return;
            }

            //find team
            var foundTeam = await _unitOfWork.TeamRepo.GetById(foundMeeting.TeamId);    

            //Check if role is valid to get meeting
            if (bypassRoles.Contains(request.UserRole))
            {
                //If lecturer
                if(request.UserRole == RoleConstants.LECTURER)
                {
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
