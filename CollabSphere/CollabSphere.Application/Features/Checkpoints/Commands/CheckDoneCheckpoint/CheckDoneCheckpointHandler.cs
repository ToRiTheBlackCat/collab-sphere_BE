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

            // Check if is student in team
            var member = checkpoint.TeamMilestone.Team.ClassMembers
                .FirstOrDefault(mem => mem.StudentId == request.UserId);
            if (member == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not a member of the team with ID: {checkpoint.TeamMilestone.Team.TeamId}"
                });
                return;
            }
        }
    }
}
