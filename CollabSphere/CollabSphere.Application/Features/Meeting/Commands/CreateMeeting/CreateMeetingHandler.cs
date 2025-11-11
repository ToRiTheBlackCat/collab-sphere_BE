using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Commands.CreateMeeting
{
    public class CreateMeetingHandler : CommandHandler<CreateMeetingCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configure;
        public CreateMeetingHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _emailSender = new EmailSender(_configure);
        }


        protected override async Task<CommandResult> HandleCommand(CreateMeetingCommand request, CancellationToken cancellationToken)
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

                //Find team
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam != null)
                {
                    //Find request user
                    var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);
                    if (foundUser != null)
                    {
                        //Create new meeting
                        var newMeeting = new Domain.Entities.Meeting
                        {
                            TeamId = request.TeamId,
                            Title = request.Title.Trim(),
                            Description = request.Description?.Trim(),
                            CreatedBy = request.UserId,
                            CreatorName = foundUser.IsTeacher ? foundUser.Lecturer.Fullname : foundUser.Student.Fullname,
                            MeetingUrl = request.MeetingUrl,
                            ScheduleTime = request.ScheduleTime,
                            CreatedAt = DateTime.UtcNow,
                            Status = (int)((request.ScheduleTime == DateTime.UtcNow) ? MeetingStatus.ACTIVE : MeetingStatus.UPCOMING),
                        };

                        await _unitOfWork.MeetingRepo.Create(newMeeting);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        //Send email for member
                        var teamMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(request.TeamId);
                        if (teamMembers != null)
                        {
                            var toEmails = new List<string>();
                            foreach (var member in teamMembers)
                            {
                                var student = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(member.StudentId);

                                toEmails.Add(student?.Email ?? "");
                            }

                            var requesterName = "";
                            if (foundUser.IsTeacher)
                            {
                                requesterName = foundUser.Lecturer.Fullname;
                            }
                            else
                            {
                                requesterName = foundUser.Student.Fullname;
                            }

                                _emailSender.SendScheduleMeetingMails(toEmails, newMeeting, request.MeetingUrl, requesterName);
                        }

                        result.IsSuccess = true;
                        result.Message = $"Create meeting with ID: {newMeeting.MeetingId} successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateMeetingCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Find Team
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
            //Check if role is valid to delete team
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
                    if(request.UserRole == RoleConstants.STUDENT)
                    {
                        //Check if team member
                        var foundTeamMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(request.TeamId, request.UserId);
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
                        if(request.UserId != foundTeam.LecturerId)
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
                        Message = $"Invalid time. Please create schedule start for now"
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
