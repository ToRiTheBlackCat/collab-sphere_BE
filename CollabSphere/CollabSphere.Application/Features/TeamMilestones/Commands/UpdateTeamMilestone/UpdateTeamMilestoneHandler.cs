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
                var milestone = (await _unitOfWork.TeamMilestoneRepo.GetById(request.TeamMilestoneDto.TeamMilestoneId))!;

                // Update milestone
                milestone.StartDate = request.TeamMilestoneDto.StartDate;
                milestone.EndDate = request.TeamMilestoneDto.EndDate;

                // Update other fields if is not original milestone
                if (!milestone.SyllabusMilestoneId.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(request.TeamMilestoneDto.Title))
                    {
                        milestone.Title = request.TeamMilestoneDto.Title;
                    }

                    if (!string.IsNullOrWhiteSpace(request.TeamMilestoneDto.Description))
                    {
                        milestone.Description = request.TeamMilestoneDto.Description; 
                    }
                }

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
            var milestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(dto.TeamMilestoneId);
            if (milestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.TeamMilestoneId),
                    Message = $"No team milestone with ID: {dto.TeamMilestoneId}",
                });
                return;
            }
            var team = milestone.Team;

            // Only the lecturer of the class can update team milestone
            if (team.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class '{team.Class.ClassName}'({team.Class.ClassId}).",
                });
                return;
            }

            // Can only update unevaluated milestone
            var milestoneEval = await _unitOfWork.MilestoneEvaluationRepo
                .GetEvaluationOfMilestone(milestone.TeamMilestoneId, team.LecturerId, team.TeamId);
            if (milestoneEval != null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.TeamMilestoneId),
                    Message = $"Can not change status of milestone '{milestone.Title}'({milestone.TeamMilestoneId}) for it has already been evaluated.",
                });
                return;
            }

            // Can only update title & description for an original milestone
            if (milestone.SyllabusMilestoneId.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(dto.Title))
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(dto.Title),
                        Message = $"Can't change the Title of a original milestone.",
                    });
                }
                if (!string.IsNullOrWhiteSpace(dto.Description))
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(dto.Description),
                        Message = $"Can't change the Description of a original milestone.",
                    });
                }
                if (errors.Any())
                {
                    return;
                }
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
            if (milestone.Checkpoints.Any())
            {
                // Check StartDate
                var earliestStartDate = milestone.Checkpoints.Min(x => x.StartDate);
                if (earliestStartDate.HasValue && dto.StartDate > earliestStartDate)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(dto.StartDate),
                        Message = $"StartDate can't be a date later than earliest checkpoint's StartDate: {earliestStartDate}",
                    });
                }

                // Check EndDate
                var latestDueDate = milestone.Checkpoints.Max(x => x.DueDate);
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
