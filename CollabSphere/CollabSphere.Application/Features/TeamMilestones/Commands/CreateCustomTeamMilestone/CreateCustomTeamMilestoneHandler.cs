using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone
{
    public class CreateCustomTeamMilestoneHandler : CommandHandler<CreateCustomTeamMilestoneCommand, CreateCustomTeamMilestoneResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCustomTeamMilestoneHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CreateCustomTeamMilestoneResult> HandleCommand(CreateCustomTeamMilestoneCommand request, CancellationToken cancellationToken)
        {
            var result = new CreateCustomTeamMilestoneResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Create new team milestone
                var newMilestone = new TeamMilestone()
                {
                    TeamId = request.MilestoneDto.TeamId,
                    Title = request.MilestoneDto.Title,
                    Description = request.MilestoneDto.Description,
                    StartDate = request.MilestoneDto.StartDate,
                    EndDate = request.MilestoneDto.EndDate,
                    Progress = 0,
                };

                await _unitOfWork.TeamMilestoneRepo.Create(newMilestone);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = "Created successfully team milestone.";
                result.TeamMilestoneId = newMilestone.TeamMilestoneId;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateCustomTeamMilestoneCommand request)
        {
            var msDto = request.MilestoneDto;

            // Check TeamId
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(msDto.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(msDto.TeamId),
                    Message = $"No team with ID \"{msDto.TeamId}\".",
                });
                return;
            }

            // Check lecturer is assigned to class
            if (request.UserId != team.Class.LecturerId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID \"{team.Class.ClassId}\".",
                });
                return;
            }

            // Check new start date & end date
            if (msDto.StartDate > msDto.EndDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(msDto.EndDate),
                    Message = $"EndDate has to be a Date after StartDate.",
                });
                return;
            }
        }
    }
}
