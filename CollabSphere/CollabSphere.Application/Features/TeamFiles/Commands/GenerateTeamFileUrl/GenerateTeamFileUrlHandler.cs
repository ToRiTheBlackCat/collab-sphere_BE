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

namespace CollabSphere.Application.Features.TeamFiles.Commands.GenerateTeamFileUrl
{
    public class GenerateTeamFileUrlHandler : CommandHandler<GenerateTeamFileUrlCommand, GenerateTeamFileUrlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public GenerateTeamFileUrlHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<GenerateTeamFileUrlResult> HandleCommand(GenerateTeamFileUrlCommand request, CancellationToken cancellationToken)
        {
            var result = new GenerateTeamFileUrlResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Get teamFile file
                var teamFile = await _unitOfWork.TeamFileRepo.GetById(request.FileId);

                // Get new pre-signed URL
                var currentTime = DateTime.UtcNow;
                var urlResponse = await _s3Client.GetPresignedUrlFromS3Async(teamFile!.ObjectKey, currentTime);

                // Update database entry
                teamFile.FileUrl = urlResponse.url;
                teamFile.UrlExpireTime = urlResponse.expireTime;

                _unitOfWork.TeamFileRepo.Update(teamFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Return updated milestone file
                result.FileUrl = teamFile.FileUrl;
                result.UrlExpireTime = teamFile.UrlExpireTime;
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

        protected override async Task ValidateRequest(List<OperationError> errors, GenerateTeamFileUrlCommand request)
        {
            // Check TeamId
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"No team with ID '{request.TeamId}'.",
                });
                return;
            }

            // Check if is lecturer of class
            if (request.UserRole == RoleConstants.LECTURER)
            {
                if (team.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{team.ClassId}'.",
                    });
                    return;
                }
            }
            // Check if is member of team
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                var isTeamMember = team.ClassMembers.Any(x => x.StudentId == request.UserId);
                if (!isTeamMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{team.TeamId}'.",
                    });
                    return;
                }
            }

            // Check team file
            var fileExist = team.TeamFiles.Any(x => x.FileId == request.FileId);
            if (!fileExist)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"No file with ID '{request.FileId}' in team files.",
                });
                return;
            }
        }
    }
}
