using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CreateCheckpoint
{
    public class CreateCheckpointHandler : CommandHandler<CreateCheckpointCommand, CreateCheckpointResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCheckpointHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CreateCheckpointResult> HandleCommand(CreateCheckpointCommand request, CancellationToken cancellationToken)
        {
            var result = new CreateCheckpointResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Construct new checkpoint
                var newCheckpoint = new Checkpoint()
                {
                    TeamMilestoneId = request.TeamMilestoneId,
                    Title = request.Title,
                    Description = request.Description,
                    Complexity = request.Complexity,
                    StartDate = request.StartDate,
                    DueDate = request.DueDate,
                    Status = (int)CheckpointStatuses.NOT_DONE, // Set default status
                };
                await _unitOfWork.CheckpointRepo.Create(newCheckpoint);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Created checkpoint successfully, CheckpointId: {newCheckpoint.CheckpointId}";
                result.CheckpointId = newCheckpoint.CheckpointId;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateCheckpointCommand request)
        {
            // Check teamMilesotneId
            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailsById(request.TeamMilestoneId);
            if (teamMilestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"No team milestone with ID: {request.TeamMilestoneId}"
                });
                return;
            }

            // Check if user is team member
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

            // Check start date
            if (request.StartDate.HasValue)
            {
                // Check if is valid start date
                if (request.StartDate > request.DueDate)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.StartDate),
                        Message = $"StartDate can't be a date before DueDate: {request.DueDate}"
                    });
                }

                // Check if in range of milestone dates
                if (request.StartDate < teamMilestone.StartDate)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.StartDate),
                        Message = $"StartDate can't be a date before milestone's StartDate: {teamMilestone.StartDate}"
                    });
                }
            }

            // Check if DueDate is in range of milestone dates
            if (request.DueDate > teamMilestone.EndDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.StartDate),
                    Message = $"DueDate can't be a date after milestone's EndDate: {teamMilestone.EndDate}"
                });
            }
            else if (!request.StartDate.HasValue && request.DueDate < teamMilestone.StartDate)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.StartDate),
                    Message = $"DueDate can't be a date before milestone's StartDate: {teamMilestone.StartDate}"
                });
            }
        }
    }
}
