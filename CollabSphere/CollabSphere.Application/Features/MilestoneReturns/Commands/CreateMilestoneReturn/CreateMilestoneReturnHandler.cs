using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.MilestoneFiles.Commands.UploadMilestoneFile;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.MilestoneReturns.Commands.CreateMilestoneReturn
{
    public class CreateMilestoneReturnHandler : CommandHandler<CreateMilestoneReturnCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public CreateMilestoneReturnHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(CreateMilestoneReturnCommand request, CancellationToken cancellationToken)
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
                // Upload return file to AWS
                var currentTime = DateTime.UtcNow;

                var uploadResponse = await _s3Client.UploadFileToS3Async(
                    request.File,
                    AwsS3HelperPaths.MilestoneReturn,
                    request.TeamMilestoneId,
                    currentTime
                );

                // Create database entry
                var newMileReturn = new MilestoneReturn()
                {
                    TeamMilestoneId = request.TeamMilestoneId,
                    UserId = request.UserId,
                    FileName = uploadResponse.FileName,
                    Type = request.File.ContentType,
                    FileSize = request.File.Length,
                    SubmitedDate = currentTime,
                    ObjectKey = uploadResponse.ObjectKey,
                    FileUrl = uploadResponse.PresignedUrl,
                    UrlExpireTime = uploadResponse.UrlExpireTime,
                };

                await _unitOfWork.MilestoneReturnRepo.Create(newMileReturn);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Submitted milestone return '{newMileReturn.FileName}' ({newMileReturn.MileReturnId}) to milestone with ID '{request.TeamMilestoneId}'.";
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

        protected override async Task ValidateRequest(List<OperationError> errors, CreateMilestoneReturnCommand request)
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

            if (tMilestone.MilestoneEvaluation != null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Milestone is evaluated, can not submit more returns.",
                });
                return;
            }

            if (tMilestone.Status != (int)TeamMilestoneStatuses.NOT_DONE)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Team milestone's status is not '{TeamMilestoneStatuses.NOT_DONE.ToString()}', can not submit more returns.",
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

            // Validate file
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
