using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.MilestoneFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneFiles.Commands.GenerateMilestoneFileUrl
{
    public class GenerateMilestoneFileUrlHandler : CommandHandler<GenerateMilestoneFileUrlCommand, GenerateMilestoneFileUrlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public GenerateMilestoneFileUrlHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<GenerateMilestoneFileUrlResult> HandleCommand(GenerateMilestoneFileUrlCommand request, CancellationToken cancellationToken)
        {
            var result = new GenerateMilestoneFileUrlResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Get milestone file
                var mFile = await _unitOfWork.MilestoneFileRepo.GetById(request.FileId);

                // Get new pre-signed URL
                var currentTime = DateTime.UtcNow;
                var urlResponse = await _s3Client.GetPresignedUrlFromS3Async(mFile!.ObjectKey, currentTime);

                // Update database entry
                mFile.FileUrl = urlResponse.url;
                mFile.UrlExpireTime = urlResponse.expireTime;

                _unitOfWork.MilestoneFileRepo.Update(mFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Return updated milestone file
                result.FileUrl = mFile.FileUrl;
                result.UrlExpireTime = mFile.UrlExpireTime;
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

        protected override async Task ValidateRequest(List<OperationError> errors, GenerateMilestoneFileUrlCommand request)
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

            // Requester is Lecturer
            if (request.UserRole == RoleConstants.LECTURER)
            {
                // Check if is class's assigned lecturer
                if (request.UserId != tMilestone.Team.Class.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{tMilestone.Team.Class.ClassId}'.",
                    });
                    return;
                } 
            }
            // Requester is Student
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                // Check if is member of team
                var isTeamMember = tMilestone.Team.ClassMembers.Select(cm => cm.StudentId).Contains(request.UserId);
                if (!isTeamMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{tMilestone.Team.TeamId}'.",
                    });
                    return;
                }
            }

            // Check milestone file
            var mFile = await _unitOfWork.MilestoneFileRepo.GetById(request.FileId);
            if (mFile == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"No milestone file with ID {request.FileId}.",
                });
                return;
            }
        }
    }
}
