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

            // Check teamMilesotneId
            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(dto.TeamMilestoneId);
            if (teamMilestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(dto.TeamMilestoneId),
                    Message = $"No team milestone with ID: {dto.TeamMilestoneId}"
                });
                return;
            }

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

            // Get milestone for validation
            var classEntity = teamMilestone!.Team.Class;
            var team = teamMilestone.Team;

            // Can not update checkpoint if milestone is evaluated
            if (teamMilestone.MilestoneEvaluation != null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UpdateDto.CheckpointId),
                    Message = $"Can not update checkpoint. Reason - The milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) has already been evaluated.",
                });
                return;
            }

            // Can not update checkpoint if milestone's status is DONE
            if (teamMilestone.Status == (int)TeamMilestoneStatuses.DONE)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UpdateDto.CheckpointId),
                    Message = $"Can not update checkpoint. Reason - The milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) status is DONE.",
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
                    Field = nameof(dto.DueDate),
                    Message = $"DueDate can't be a date after milestone's EndDate: {teamMilestone.EndDate}"
                });
            }
        }
    }
}
