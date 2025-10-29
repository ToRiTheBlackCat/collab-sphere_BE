using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Checkpoints.Commands.CheckpointUploadFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CheckpointDeleteFile
{
    public class CheckpointDeleteFileHandler : CommandHandler<CheckpointDeleteFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public CheckpointDeleteFileHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(CheckpointDeleteFileCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.RollbackTransactionAsync();

                #region Data Operation
                var checkpointFile = (await _unitOfWork.CheckpointFileRepo.GetById(request.FileId))!;

                // Delete file on Aws S3
                await _s3Client.DeleteFileFromS3Async(checkpointFile.ObjectKey);

                // Delete file entry in DB
                _unitOfWork.CheckpointFileRepo.Delete(checkpointFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.RollbackTransactionAsync();

                result.Message = $"Deleted successlly checkpoint file \"{checkpointFile.FileName}\".";
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

        protected override async Task ValidateRequest(List<OperationError> errors, CheckpointDeleteFileCommand request)
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

            if (request.UserRole == RoleConstants.STUDENT)
            {
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
            else if (request.UserRole == RoleConstants.LECTURER && checkpoint.TeamMilestone.Team.LecturerId != request.UserId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{checkpoint.TeamMilestone.Team.ClassId}'",
                });
                return;
            }

            // Check file is in checkpoint
            if (checkpoint.CheckpointFiles
                .FirstOrDefault(x => x.FileId == request.FileId) == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"Checkpoint ({request.CheckpointId}) doesn't have any file with ID: {request.FileId}",
                });
                return;
            }
        }
    }
}
