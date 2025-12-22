using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CheckDoneCheckpoint
{
    public class CheckDoneCheckpointHandler : CommandHandler<CheckDoneCheckpointCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckDoneCheckpointHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CheckDoneCheckpointCommand request, CancellationToken cancellationToken)
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
                // Get checkpoint
                var checkpoint = (await _unitOfWork.CheckpointRepo.GetById(request.CheckpointId))!;
                checkpoint.Status = (int)(request.IsDone ? CheckpointStatuses.DONE : CheckpointStatuses.NOT_DONE);

                _unitOfWork.CheckpointRepo.Update(checkpoint);
                await _unitOfWork.SaveChangesAsync();

                var milestone = (await _unitOfWork.TeamMilestoneRepo.GetById(checkpoint.TeamMilestoneId))!;
               
                #endregion

                #region Calculate Progress Info Of Team 
                var foundTeam = await _unitOfWork.TeamRepo.GetTeamDetail(milestone.TeamId);
                var milestones = foundTeam.TeamMilestones?.ToList() ?? new List<Domain.Entities.TeamMilestone>();
                var totalMilestones = milestones.Count;
                var completedMilestones = milestones.Count(m => m.Status == (int)TeamMilestoneStatuses.DONE);

                var checkpoints = milestones.SelectMany(m => m.Checkpoints ?? new List<Domain.Entities.Checkpoint>()).ToList();
                var totalCheckpoints = checkpoints.Count;
                var completedCheckpoints = checkpoints.Count(c => c.Status == (int)CheckpointStatuses.DONE);

                var overallProgress = (totalMilestones + totalCheckpoints) > 0
                        ? (float)Math.Round(
                            ((completedMilestones + completedCheckpoints) * 100f) /
                            (totalMilestones + totalCheckpoints), 2)
                        : 0;

                //Update Total progress of team
                var teamEntity = await _unitOfWork.TeamRepo.GetById(milestone.TeamId);

                teamEntity.Progress = overallProgress;
                _unitOfWork.TeamRepo.Update(teamEntity);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Updated status for checkpoint with ID: {checkpoint.CheckpointId}";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CheckDoneCheckpointCommand request)
        {
            // Check checkpointId
            var checkpoint = await _unitOfWork.CheckpointRepo.GetCheckpointDetail(request.CheckpointId);
            if (checkpoint == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.CheckpointId),
                    Message = $"No checkpoint with ID: {request.CheckpointId}",
                });
                return;
            }

            // Get milestone for validation
            var milestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(checkpoint.TeamMilestoneId);
            var classEntity = milestone!.Team.Class;
            var team = milestone.Team;

            // Can not check done if milestone is evaluated
            if (milestone.MilestoneEvaluation != null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.CheckpointId),
                    Message = $"Can not change the checkpoint status. Reason - The milestone '{milestone.Title}'({milestone.TeamMilestoneId}) has already been evaluated.",
                });
                return;
            }

            // Can not check done if milestone's status is DONE
            if (milestone.Status == (int)TeamMilestoneStatuses.DONE)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.CheckpointId),
                    Message = $"Can not change the checkpoint status. Reason - The milestone '{milestone.Title}'({milestone.TeamMilestoneId}) status is DONE.",
                });
                return;
            }

            // Lecturer have to be assigned to class
            if (request.UserRole == RoleConstants.LECTURER)
            {
                if (classEntity.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You({request.UserId}) are not the assigned lecturer of class '{classEntity.ClassName}'({classEntity.ClassId}).",
                    });
                    return;
                }
            }
            // Student have to be team member
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                var isMember = team.ClassMembers
                    .Any(mem => mem.StudentId == request.UserId);
                if (!isMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You({request.UserId}) are not a member of the team '{team.TeamName}'({team.TeamId})."
                    });
                    return;
                }
            }
        }
    }
}
