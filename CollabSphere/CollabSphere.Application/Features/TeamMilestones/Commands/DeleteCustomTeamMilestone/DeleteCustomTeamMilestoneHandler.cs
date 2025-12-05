using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.DeleteCustomTeamMilestone
{
    public class DeleteCustomTeamMilestoneHandler : CommandHandler<DeleteCustomTeamMilestoneCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCustomTeamMilestoneHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteCustomTeamMilestoneCommand request, CancellationToken cancellationToken)
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

                // Get team milestone
                var milestone = (await _unitOfWork.TeamMilestoneRepo.GetById(request.TeamMilestoneId))!;

                milestone.Status = (int)TeamMilestoneStatuses.SOFT_DELETED;

                _unitOfWork.TeamMilestoneRepo.Update(milestone);
                await _unitOfWork.SaveChangesAsync();

                #region Update progress of team
                // Also Update Team's progress
                var team = await _unitOfWork.TeamRepo.GetById(milestone.TeamId);
                var teamMilestones = await _unitOfWork.TeamMilestoneRepo.GetMilestonesByTeamId(team!.TeamId);
                var doneCount = teamMilestones.Count(x => x.Status == (int)TeamMilestoneStatuses.DONE);
                team.Progress = MathF.Round((doneCount * 1.0f) / teamMilestones.Count * 100f, 2);

                _unitOfWork.TeamRepo.Update(team);
                await _unitOfWork.SaveChangesAsync(); 
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted successfully team milestone '{milestone.Title}' ({milestone.TeamMilestoneId}).";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteCustomTeamMilestoneCommand request)
        {
            // Check milestone
            var milestone = await _unitOfWork.TeamMilestoneRepo.GetDetailsById(request.TeamMilestoneId);
            if (milestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"No team milestone with ID '{request.TeamMilestoneId}'."
                });
                return;
            }

            // Check requesting lecturer
            if (request.UserId != milestone.Team.Class.LecturerId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{milestone.Team.Class.ClassId}'.",
                });
                return;
            }

            // Can only delete milestones that aren't originally mapped from a project
            if (milestone.ObjectiveMilestoneId.HasValue)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Can not delete an original milestone.",
                });
                return;
            }

            // Can not delete an milestone that is evaluated
            if (milestone.MilestoneEvaluation != null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Can not delete a milestone that is already evaluated.",
                });
            }

            // Can not delete a milestone that have checkpoints
            if (milestone.Checkpoints.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Can not delete a milestone that have already implemented checkpoints.",
                });
            }
        }
    }
}
