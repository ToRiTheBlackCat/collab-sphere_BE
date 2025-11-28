using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.UpdateCheckpoint
{
    public class UpdateCheckpointHandler : CommandHandler<UpdateCheckpointCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCheckpointHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateCheckpointCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                var dto = request.UpdateDto;
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Get checkpoint
                var checkpoint = (await _unitOfWork.CheckpointRepo.GetById(dto.CheckpointId))!;

                // Update checkpoint
                checkpoint.Title = dto.Title;
                checkpoint.TeamMilestoneId = dto.TeamMilestoneId;
                checkpoint.Description = dto.Description;
                checkpoint.Complexity = dto.Complexity;
                checkpoint.StartDate = dto.StartDate;
                checkpoint.DueDate = dto.DueDate;

                _unitOfWork.CheckpointRepo.Update(checkpoint);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Updated checkpoint with ID: {checkpoint.CheckpointId}";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateCheckpointCommand request)
        {
            var dto = request.UpdateDto;

            // Check checkpointId
            var checkpoint = await _unitOfWork.CheckpointRepo.GetById(dto.CheckpointId);
            if (checkpoint == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.CheckpointId),
                    Message = $"No checkpoint with ID: {dto.CheckpointId}",
                });
                return;
            }

            // Check teamMilesotneId
            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailsById(dto.TeamMilestoneId);
            if (teamMilestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.TeamMilestoneId),
                    Message = $"No team milestone with ID: {dto.TeamMilestoneId}"
                });
                return;
            }

            // Check if user is team member
            if (request.UserRole == RoleConstants.STUDENT)
            {
                var member = teamMilestone.Team.ClassMembers
                    .FirstOrDefault(x => x.StudentId == request.UserId);
                if (member == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID: {teamMilestone.TeamId}"
                    });
                    return;
                }
            }
            // Check if user is lecturer of class
            else if (request.UserRole == RoleConstants.LECTURER && teamMilestone.Team.Class.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID: {teamMilestone.Team.Class.ClassId}"
                });
                return;
            }

            // StartDate must be before DueDate
            if (dto.StartDate > dto.DueDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.StartDate),
                    Message = $"StartDate can't be a date after DueDate: {dto.DueDate}"
                });
            }

            // StartDate must be >= milestone StartDate
            if (dto.StartDate < teamMilestone.StartDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.StartDate),
                    Message = $"StartDate can't be a date before milestone's StartDate: {teamMilestone.StartDate}"
                });
            }

            // DueDate must be <= milestone EndDate
            if (dto.DueDate > teamMilestone.EndDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.StartDate),
                    Message = $"DueDate can't be a date after milestone's EndDate: {teamMilestone.EndDate}"
                });
            }
        }
    }
}
