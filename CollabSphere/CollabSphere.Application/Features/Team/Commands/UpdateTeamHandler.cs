using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands
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
                //Project Id
                if (request.ProjectAssignmentId.HasValue && request.ProjectAssignmentId.Value != 0)
                    foundTeam.ProjectAssignmentId = request.ProjectAssignmentId;

                if (request.EndDate.HasValue)
                    foundTeam.EndDate = request.EndDate;

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
            //Validate class
            var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (foundClass == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ClassId),
                    Message = $"Not found any class with that Id: {request.ClassId}"
                });
            }

            //Check project assignment exists in class
            if (request.ProjectAssignmentId.HasValue)
            {
                var projectAssignment = _unitOfWork.ProjectAssignmentRepo.GetById(request.ProjectAssignmentId ?? 0).Result;
                if (projectAssignment == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = "InvalidProjectAssignment",
                        Message = $"Project assignment with ID {request.ProjectAssignmentId} does not exist."
                    });
                }
                if (projectAssignment != null && projectAssignment.ClassId != request.ClassId)
                {
                    errors.Add(new OperationError
                    {
                        Field = "ProjectAssignmentClassMismatch",
                        Message = $"Project assignment with ID {request.ProjectAssignmentId} does not belong to class with ID {request.ClassId}."
                    });
                }
            }

            //Check end date is after start date
            var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
            if (foundTeam == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamId),
                    Message = $"Not found any team with that Id: {request.TeamId}"
                });
            }

            //Check end date is after start date
            if (request.EndDate.HasValue && foundTeam != null && request.EndDate <= foundTeam.CreatedDate)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.EndDate),
                    Message = "End date must be after created date."
                });
            }

        }
    }
}
