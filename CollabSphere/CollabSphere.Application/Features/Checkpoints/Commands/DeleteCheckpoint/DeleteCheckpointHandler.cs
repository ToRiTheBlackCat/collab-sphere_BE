using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Checkpoints.Commands.DeleteCheckpoint
{
    public class DeleteCheckpointHandler : CommandHandler<DeleteCheckpointCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public DeleteCheckpointHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
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
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        _unitOfWork.CheckpointFileRepo.Delete(file);
                    }
                    await _unitOfWork.SaveChangesAsync();

                    var deleteResponse = await _s3Client.DeleteFilesFromS3Async(files.Select(x => x.ObjectKey));
                }

                // Remove checkpoint
                var checkpoint = (await _unitOfWork.CheckpointRepo.GetById(request.CheckpointId))!;
                _unitOfWork.CheckpointRepo.Delete(checkpoint);
                await _unitOfWork.SaveChangesAsync();

                // Also update milestone progress
                var milestone = (await _unitOfWork.TeamMilestoneRepo.GetById(checkpoint.TeamMilestoneId))!;
                var checkpointsOfMilestone = await _unitOfWork.CheckpointRepo.GetCheckpointsByMilestone(checkpoint.TeamMilestoneId);
                if (checkpointsOfMilestone.Any())
                {
                    var doneCount = checkpointsOfMilestone.Count(x => x.Status == (int)CheckpointStatuses.DONE);
                    milestone.Progress = (doneCount * 1.0f / checkpointsOfMilestone.Count) * 100.0f;
                    _unitOfWork.TeamMilestoneRepo.Update(milestone);
                    await _unitOfWork.SaveChangesAsync();
                }
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted successfully checkpoint '{checkpoint.Title}' with ID: {checkpoint.CheckpointId}";
                result.IsSuccess = true;
            }
            catch (AmazonS3Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Handle cases like the object key not being found
                result.Message = $"S3 Error: {ex.Message}";
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

            // Get milestone for validation
            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(checkpoint.TeamMilestoneId);
            var classEntity = teamMilestone!.Team.Class;
            var team = teamMilestone.Team;

            // Can not delete checkpoint if milestone is evaluated
            if (teamMilestone.MilestoneEvaluation != null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.CheckpointId),
                    Message = $"Can not delete checkpoint. Reason - The milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) has already been evaluated.",
                });
                return;
            }

            // Can not delete checkpoint if milestone's status is DONE
            if (teamMilestone.Status == (int)TeamMilestoneStatuses.DONE)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.CheckpointId),
                    Message = $"Can not delete checkpoint. Reason - The milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) status is DONE.",
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
