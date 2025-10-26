using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.UpdateTeamMilestone
{
    public class UpdateTeamMilestoneHandler : CommandHandler<UpdateTeamMilestoneCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTeamMilestoneHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateTeamMilestoneCommand request, CancellationToken cancellationToken)
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

                #region Data Operation
                // Get milestone
                var milestone = await _unitOfWork.TeamMilestoneRepo.GetById(request.TeamMilestoneDto.TeamMilestoneId);
                milestone!.Team = null;
                milestone!.Checkpoints = null;
                

                // Update milestone
                milestone!.StartDate = request.TeamMilestoneDto.StartDate;
                milestone.StartDate = request.TeamMilestoneDto.EndDate;
                _unitOfWork.TeamMilestoneRepo.Update(milestone);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Updated team milestone with ID: {milestone.TeamMilestoneId}";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateTeamMilestoneCommand request)
        {
            var dto = request.TeamMilestoneDto;

            // Check TeamMilestoneId
            var milestone = await _unitOfWork.TeamMilestoneRepo.GetById(dto.TeamMilestoneId);
            if (milestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.TeamMilestoneId),
                    Message = $"No team milestone with ID: {dto.TeamMilestoneId}",
                });
                return;
            }

            if (milestone.Team.Class.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID: {milestone.Team.Class.ClassId}.",
                });
                return;
            }

            // Check new start date & end date
            if (dto.StartDate > dto.EndDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.EndDate),
                    Message = $"EndDate has to be after StartDate.",
                });
                return;
            }

            // Get checkpoints in milestone
            var milestoneCheckpoints = milestone.Checkpoints.OrderBy(mls => mls.StartDate);
            if (milestoneCheckpoints.Any())
            {
                // Check StartDate
                var earliestStartDate = milestoneCheckpoints.Min(x => x.StartDate);
                if (earliestStartDate.HasValue && dto.StartDate > earliestStartDate)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(dto.StartDate),
                        Message = $"StartDate can't be a date later than earliest checkpoint's StartDate: {earliestStartDate}",
                    });
                }

                // Check EndDate
                var latestDueDate = milestoneCheckpoints.Max(x => x.DueDate);
                if (dto.EndDate < latestDueDate)
                {   
                    errors.Add(new OperationError()
                    {
                        Field = nameof(dto.EndDate),
                        Message = $"EndDate can't be a date earlier than latest checpoint's DueDate: {latestDueDate}",
                    });
                }
            }
        }
    }
}
