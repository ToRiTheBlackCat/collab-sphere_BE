using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Commands.DeleteMeeting
{

    public class DeleteMeetingHandler : CommandHandler<DeleteMeetingCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteMeetingHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteMeetingCommand request, CancellationToken cancellationToken)
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

                //Find meeting
                var foundMeeting = await _unitOfWork.MeetingRepo.GetById(request.MeetingId);
                if (foundMeeting != null)
                {
                    _unitOfWork.MeetingRepo.Delete(foundMeeting);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Delete meeting with ID: {request.MeetingId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteMeetingCommand request)
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
            else
            {
                if (foundMeeting.ScheduleTime <= DateTime.UtcNow)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.MeetingId),
                        Message = "Cannot delete meeting that in the past"
                    });
                    return;
                }
            }

            var foundTeam = await _unitOfWork.TeamRepo.GetById(foundMeeting.TeamId);

            //Check if role is valid to delete meeting
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
                        //Check if creator of meeting

                        if (request.UserId != foundMeeting.CreatedBy)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the user that create this meeting. Cannot delete this meeting"
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
                                Message = $"You are not the lecturer of this team. Cannot delete meeting for this team"
                            });
                            return;
                        }
                    }
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
