using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone
{
    public class CheckTeamMilestoneHandler : CommandHandler<CheckTeamMilestoneCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckTeamMilestoneHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CheckTeamMilestoneCommand request, CancellationToken cancellationToken)
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
                var milestone = await _unitOfWork.TeamMilestoneRepo.GetById(request.CheckDto.TeamMilestoneId);
                milestone!.Team = null;
                milestone!.Checkpoints = null;

                // Update milestone status
                milestone!.Status = (int)(request.CheckDto.IsDone ? TeamMilestoneStatuses.DONE : TeamMilestoneStatuses.NOT_DONE);
                _unitOfWork.TeamMilestoneRepo.Update(milestone);
                await _unitOfWork.SaveChangesAsync();

                // Also Update Team's progress
                var team = await _unitOfWork.TeamRepo.GetById(milestone.TeamId);
                var teamMilestones = await _unitOfWork.TeamMilestoneRepo.GetMilestonesByTeamId(team!.TeamId);
                var doneCount = teamMilestones.Count(x => x.Status == (int)TeamMilestoneStatuses.DONE);
                team.Progress = MathF.Round((doneCount * 1.0f) / teamMilestones.Count * 100f, 2);

                _unitOfWork.TeamRepo.Update(team);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Updated status of team milestone with ID: {milestone.TeamMilestoneId}";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CheckTeamMilestoneCommand request)
        {
            var dto = request.CheckDto;

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

            // Only the team leader can change milestone's status
            var member = team.ClassMembers
                .FirstOrDefault(mem => mem.StudentId == request.UserId);
            if (member == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not a member of the team '{team.TeamName}'({team.TeamId}).",
                });
                return;
            }
            else if (member.TeamRole != (int)TeamRole.LEADER)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the LEADER the team '{team.TeamName}'({team.TeamId}).",
                });
                return;
            }

            // Can only change status of an unevaluated milestone
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
        }
    }
}
