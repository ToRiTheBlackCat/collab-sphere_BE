using Amazon.S3;
using Amazon.S3.Model;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CheckpointUploadFile
{
    public class CheckpointUploadHandler : CommandHandler<CheckpointUploadCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public CheckpointUploadHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(CheckpointUploadCommand request, CancellationToken cancellationToken)
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
                // Upload file to AWS
                var currentTime = DateTime.UtcNow;

                var uploadResponse = await _s3Client.UploadFileToS3Async(
                    request.File,
                    AwsS3HelperPaths.Checkpoint,
                    request.CheckpointId,
                    currentTime
                );

                // Create database entry
                var checkFile = new CheckpointFile()
                {
                    CheckpointId = request.CheckpointId,
                    UserId = request.UserId,
                    FileName = uploadResponse.FileName,
                    Type = request.File.ContentType,
                    FileUrl = uploadResponse.PresignedUrl,
                    FileSize = request.File.Length,
                    ObjectKey = uploadResponse.ObjectKey,
                    CreatedAt = currentTime,
                    UrlExpireTime = uploadResponse.UrlExpireTime,
                };

                await _unitOfWork.CheckpointFileRepo.Create(checkFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Uploaded file \"{uploadResponse.FileName}\" successfully.";
                result.IsSuccess = true;
            }
            catch (AmazonS3Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Handle cases like the object key not being found
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    result.Message = $"S3 Error: {ex.Message}";
                }
                else
                {
                    result.Message = $"S3 Error: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CheckpointUploadCommand request)
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

            // Check file
            if (!FileValidator.ValidateFile(request.File, out var errorMessage))
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.File),
                    Message = errorMessage,
                });
                return;
            }
        }
    }
}
