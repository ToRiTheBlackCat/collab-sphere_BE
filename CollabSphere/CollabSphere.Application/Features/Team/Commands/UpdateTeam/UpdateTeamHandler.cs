using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.UpdateTeam
{
    public class UpdateTeamHandler : CommandHandler<UpdateTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTeamHandler> _logger;

        public UpdateTeamHandler(IUnitOfWork unitOfWork,
                                 ILogger<UpdateTeamHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateTeamCommand request, CancellationToken cancellationToken)
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

                //Find existed team
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null)
                {
                    result.Message = $"Cannot find any team with that Id: {request.TeamId}";

                    return result;
                }

                //Update team
                foundTeam.TeamName = request.TeamName.Trim();
                //Enrol Key
                foundTeam.EnrolKey = string.IsNullOrWhiteSpace(request.EnrolKey)
                    ? foundTeam.EnrolKey
                    : request.EnrolKey.Trim();
                //Description
                foundTeam.Description = string.IsNullOrWhiteSpace(request.Description)
                    ? foundTeam.Description
                    : request.Description.Trim();
                //Git Link
                foundTeam.GitLink = string.IsNullOrWhiteSpace(request.GitLink)
                    ? foundTeam.GitLink
                    : request.GitLink.Trim();

                _unitOfWork.TeamRepo.Update(foundTeam);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = $"Team '{foundTeam.TeamName}' updated successfully.";
                _logger.LogInformation("Team updated successfully with ID: {TeamId}", request.TeamId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while updating team.");
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check team exists
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
                    if (request.UserRole == foundTeam.LecturerId)
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This lecturer with ID: {request.UserId} not has permission to update this team."
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
                    if (request.UserRole == foundTeam.LeaderId)
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This student with ID: {request.UserId} is not the leader - not has permission to update this team."
                        });
                    return;
                }
            }
        }
    }
}
