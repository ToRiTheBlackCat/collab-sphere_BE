using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.GenerateCheckpointFileUrl
{
    public class GenerateCheckpointFileUrlHandler : CommandHandler<GenerateCheckpointFileUrlCommand, GenerateCheckpointFileUrlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public GenerateCheckpointFileUrlHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<GenerateCheckpointFileUrlResult> HandleCommand(GenerateCheckpointFileUrlCommand request, CancellationToken cancellationToken)
        {
            var result = new GenerateCheckpointFileUrlResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                // Get checkpoint
                var file = (await _unitOfWork.CheckpointFileRepo.GetById(request.FileId))!;

                //var timeGap = DateTime.UtcNow - file.UrlExpireTime;
                //if (timeGap.TotalHours > 2d)
                //{
                //    result.FileUrl = file.FileUrl;
                //    result.UrlExpireTime = file.UrlExpireTime;
                //    result.IsSuccess = true;
                //    return result;
                //}

                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Generate new presigned URL
                var presignResponse = await _s3Client.GetPresignedUrlFromS3Async(file.ObjectKey, DateTime.UtcNow);

                file.FileUrl = presignResponse.url;
                file.UrlExpireTime = presignResponse.expireTime;

                _unitOfWork.CheckpointFileRepo.Update(file);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.FileUrl = presignResponse.url;
                result.UrlExpireTime = presignResponse.expireTime;
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

        protected override async Task ValidateRequest(List<OperationError> errors, GenerateCheckpointFileUrlCommand request)
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
                // User is not lecuter assigned to class
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
