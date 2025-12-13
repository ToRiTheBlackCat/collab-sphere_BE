using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Commands.UpdateMeeting
{
    public class UpdateMeetingHandler : CommandHandler<UpdateMeetingCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateMeetingHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(UpdateMeetingCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find Meeting
                var foundMeeting = await _unitOfWork.MeetingRepo.GetById(request.MeetingId);
                if (foundMeeting != null)
                {
                    if (!string.IsNullOrEmpty(request.Title))
                    {
                        foundMeeting.Title = request.Title;
                    }
                    if (!string.IsNullOrEmpty(request.Description))
                    {
                        foundMeeting.Description = request.Description;
                    }
                    if (request.ScheduleTime.HasValue && request.ScheduleTime.Value >= DateTime.Today)
                    {
                        foundMeeting.ScheduleTime = request.ScheduleTime.Value;
                    }
                    if (!string.IsNullOrEmpty(request.RecordUrl))
                    {
                        foundMeeting.RecordUrl = request.RecordUrl;
                    }
                    if (request.Status != null)
                    {
                        foundMeeting.Status = (int)request.Status;
                    }

                    _unitOfWork.MeetingRepo.Update(foundMeeting);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Update meeting with ID: {request.MeetingId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message += ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateMeetingCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Find Team
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
            var foundTeam = await _unitOfWork.TeamRepo.GetById(foundMeeting.TeamId);

            //Check if role is valid to update meeting
            if (bypassRoles.Contains(request.UserRole))
            {
                //find user
                var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);
                if (foundUser == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"Not found any user with that Id: {request.UserId}"
                    });
                    return;
                }
                else
                {
                    //If student
                    if (request.UserRole == RoleConstants.STUDENT)
                    {
                        //Check if team member
                        var foundTeamMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.UserId);
                        if (foundTeamMem == null)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the member of this team. Cannot create meeting for this team"
                            });
                            return;
                        }
                    }
                    else
                    {
                        if (request.UserId != foundTeam.LecturerId)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the lecturer of this team. Cannot create meeting for this team"
                            });
                            return;
                        }
                    }
                }

                if (request.ScheduleTime < DateTime.UtcNow)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.ScheduleTime),
                        Message = $"Invalid time. Please create meeting that start from now"
                    });
                    return;
                }

            }
            else
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserRole),
                    Message = $"You do not have permission to use this function"
                });
                return;
            }
        }
    }
}

