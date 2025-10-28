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
                var currentTime = DateTime.UtcNow;

                // Get the original file name parts
                string fileName = Path.GetFileNameWithoutExtension(request.File.FileName);
                string extension = Path.GetExtension(request.File.FileName);
                string timestamp = currentTime.ToString("yyyyMMddHHmmss"); // Create a file-safe timestamp
                var newFileName = $"{fileName}_{timestamp}{extension}"; // New unique file name

                string folderPath = $"uploads/checkpoints/{request.CheckpointId}"; // Bucket's checkpoint folder path

                string objectKey = $"{folderPath}/{newFileName}";

                // Upload file to AWS
                await using var stream = request.File.OpenReadStream();

                var putRequest = new PutObjectRequest
                {
                    BucketName = "collab-sphere-bucket",
                    Key = objectKey,
                    InputStream = stream,
                    ContentType = request.File.ContentType,
                };

                var putObjectResponse = await _s3Client.PutObjectAsync(putRequest);

                // Generate pre-signed URL for file download
                var preSignedRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "collab-sphere-bucket",
                    Key = objectKey,
                    Expires = currentTime.AddHours(5),
                };

                var preSignedUrl = await _s3Client.GetPreSignedURLAsync(preSignedRequest);

                // Create database entry
                var checkFile = new CheckpointFile()
                {
                    CheckpointId = request.CheckpointId,
                    UserId = request.UserId,
                    FileName = newFileName,
                    Type = request.File.ContentType,
                    FilePath = preSignedUrl,
                    FileSize = request.File.Length,
                    CreatedAt = currentTime,
                    PathExpireTime = currentTime.AddHours(5),
                };

                await _unitOfWork.CheckpointFileRepo.Create(checkFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Uploaded file \"{newFileName}\" successfully.";
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
            if (!FileValidator.ValidateFile(request.File, out var errorMessage, maxFilSize: 15))
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
