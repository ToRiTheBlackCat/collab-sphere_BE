using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Project.Queries.GetProjectById;
using CollabSphere.Application.Features.User.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.DeleteTeam
{
    public class DeleteTeamHandler : CommandHandler<DeleteTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTeamHandler> _logger;

        public DeleteTeamHandler(IUnitOfWork unitOfWork,
                                 ILogger<DeleteTeamHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteTeamCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find the team
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null)
                {
                    result.Message = $"Team with the given ID: {request.TeamId} does not exist.";
                    return result;
                }

                //Soft delete the team
                foundTeam.Status = 0;
                _unitOfWork.TeamRepo.Update(foundTeam);
                await _unitOfWork.SaveChangesAsync();

                //Update data for all class members in this team
                var classMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(request.TeamId);
                if (classMembers != null && classMembers.Any())
                {
                    foreach (var member in classMembers)
                    {
                        member.TeamId = null;
                        member.TeamRole = null;
                        member.IsGrouped = false;
                        _unitOfWork.ClassMemberRepo.Update(member);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                //Set data to result
                result.IsSuccess = true;
                result.Message = "Team has been successfully deleted.";
                _logger.LogInformation("Team with ID: {TeamId} has been deleted by User with ID: {UserId}", request.TeamId, request.UserId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "An error occurred while deleting the team with ID: {TeamId} by User with ID: {UserId}", request.TeamId, request.UserId);
                result.Message = "An error occurred while processing your request.";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };
            var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);

            //Check if team exists
            if (foundTeam == null || foundTeam.Status == 0)
            {
                errors.Add(new OperationError()
                {
                    Field = "TeamId",
                    Message = $"Team with the given ID: {request.TeamId} does not exist!"
                });
                return;
            }

            //Check if role is valid to delete team
            if (bypassRoles.Contains(request.UserRole))
            {
                //If Lecturer
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
                            Message = $"This lecturer with ID: {request.UserId} not has permission to delete this team."
                        });
                    return;
                }

                //IF Student
                if (request.UserRole == RoleConstants.STUDENT)
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

                    //Check if student is the leader of the team
                    if (request.UserId != foundTeam.LeaderId)
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This student with ID: {request.UserId} is not the leader - not has permission to delete this team."
                        });
                    return;
                }
            }
        }
    }
}
