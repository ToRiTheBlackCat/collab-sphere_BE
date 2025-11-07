using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.MilestoneFiles.Commands.GenerateMilestoneFileUrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneReturns.Commands.GenerateMilestoneReturnUrl
{
    public class GenerateMilestoneReturnUrlHandler : CommandHandler<GenerateMilestoneReturnUrlCommand, GenerateMilestoneReturnUrlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public GenerateMilestoneReturnUrlHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<GenerateMilestoneReturnUrlResult> HandleCommand(GenerateMilestoneReturnUrlCommand request, CancellationToken cancellationToken)
        {
            var result = new GenerateMilestoneReturnUrlResult()
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
                var mReturn = await _unitOfWork.MilestoneReturnRepo.GetById(request.MileReturnId);

                // Get new pre-signed URL
                var currentTime = DateTime.UtcNow;
                var urlResponse = await _s3Client.GetPresignedUrlFromS3Async(mReturn!.ObjectKey, currentTime);

                // Update database entry
                mReturn.FileUrl = urlResponse.url;
                mReturn.UrlExpireTime = urlResponse.expireTime;

                _unitOfWork.MilestoneReturnRepo.Update(mReturn);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Return updated return url
                result.FileUrl = mReturn.FileUrl;
                result.UrlExpireTime = mReturn.UrlExpireTime;
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

        protected override async Task ValidateRequest(List<OperationError> errors, GenerateMilestoneReturnUrlCommand request)
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

            // Check is a member in team
            var isTeamMember = tMilestone.Team.ClassMembers
                .Any(x => x.StudentId == request.UserId);
            if (request.UserRole != RoleConstants.STUDENT || !isTeamMember)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not a member in the team with ID '{tMilestone.Team.TeamId}'.",
                });
                return;
            }

            // Validate milestone return
            var mReturn = tMilestone.MilestoneReturns
                .FirstOrDefault(x => x.MileReturnId == request.MileReturnId);
            if (mReturn == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"No milestone return with ID '{request.MileReturnId}' in milestone.",
                });
                return;
            }
        }
    }
}
