using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.DeleteCheckpoint
{
    public class DeleteCheckpointHandler : CommandHandler<DeleteCheckpointCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCheckpointHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteCheckpointCommand request, CancellationToken cancellationToken)
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

                #region Data operation
                // Remove checkpoint's member assignments
                var assignments = await _unitOfWork.CheckpointAssignmentRepo.GetByCheckpointId(request.CheckpointId);
                foreach (var assignment in assignments)
                {
                    _unitOfWork.CheckpointAssignmentRepo.Delete(assignment);
                }
                await _unitOfWork.SaveChangesAsync();

                // Remove checkpoint's files
                var files = await _unitOfWork.CheckpointFileRepo.GetFilesByCheckpointId(request.CheckpointId);
                foreach (var file in files)
                {
                    _unitOfWork.CheckpointFileRepo.Delete(file);
                }
                await _unitOfWork.SaveChangesAsync();

                // Remove checkpoint
                var checkpoint = (await _unitOfWork.CheckpointRepo.GetById(request.CheckpointId))!;
                _unitOfWork.CheckpointRepo.Delete(checkpoint);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted successfully checkpoint '{checkpoint.Title}' with ID: {checkpoint.CheckpointId}";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteCheckpointCommand request)
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

            // Check if user is team member
            if (request.UserRole == RoleConstants.STUDENT)
            {
                var member = checkpoint.TeamMilestone.Team.ClassMembers
                    .FirstOrDefault(x => x.StudentId == request.UserId);
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
            else if (request.UserRole == RoleConstants.LECTURER)
            {
                if (checkpoint.TeamMilestone.Team.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID: {checkpoint.TeamMilestone.Team.ClassId}",
                    });
                    return;
                }
            }
        }
    }
}
