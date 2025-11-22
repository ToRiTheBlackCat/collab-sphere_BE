using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.MilestoneFiles.Commands.UploadMilestoneFile
{
    public class UploadMilestoneFileHandler : CommandHandler<UploadMilestoneFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public UploadMilestoneFileHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(UploadMilestoneFileCommand request, CancellationToken cancellationToken)
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
                    AwsS3HelperPaths.Milestone,
                    request.TeamMilestoneId,
                    currentTime
                );

                // Create database entry
                var newMilestoneFile = new MilestoneFile()
                {
                    TeamMilstoneId = request.TeamMilestoneId,
                    UserId = request.UserId,
                    FileName = uploadResponse.FileName,
                    Type = request.File.ContentType,
                    FileSize = request.File.Length,
                    CreatedAt = currentTime,
                    ObjectKey = uploadResponse.ObjectKey,
                    FileUrl = uploadResponse.PresignedUrl,
                    UrlExpireTime = uploadResponse.UrlExpireTime,
                };

                await _unitOfWork.MilestoneFileRepo.Create(newMilestoneFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Uploaded file '{newMilestoneFile.FileName}' ({newMilestoneFile.FileId}) to milestone with ID '{request.TeamMilestoneId}'.";
                result.IsSuccess = true;
            }
            catch (AmazonS3Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = $"S3 Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UploadMilestoneFileCommand request)
        {
            // Check team milestone
            var tMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailsById(request.TeamMilestoneId);
            if (tMilestone == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"No team milestone with ID '{request.TeamMilestoneId}'.",
                });
                return;
            }

            // Check is class's assigned lecturer
            if (request.UserId != tMilestone.Team.Class.LecturerId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{tMilestone.Team.Class.ClassId}'.",
                });
                return;
            }

            // Validate file
            if (!FileValidator.ValidateFile(request.File, out var errorMessage, 15))
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
