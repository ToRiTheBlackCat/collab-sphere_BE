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

namespace CollabSphere.Application.Features.MilestoneReturns.Commands.DeleteMilestoneReturn
{
    public class DeleteMilestoneReturnHandler : CommandHandler<DeleteMilestoneReturnCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public DeleteMilestoneReturnHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteMilestoneReturnCommand request, CancellationToken cancellationToken)
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
                // Get milestone returm 
                var mReturn = (await _unitOfWork.MilestoneReturnRepo.GetById(request.MileReturnId))!;

                // Delete file from Aws S3
                await _s3Client.DeleteFileFromS3Async(mReturn.ObjectKey);

                // Delete database entry
                _unitOfWork.MilestoneReturnRepo.Delete(mReturn);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted milestone return '{mReturn.FileName}' ({mReturn.MileReturnId}) from milestone with ID '{request.TeamMilestoneId}'.";
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

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteMilestoneReturnCommand request)
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
                    Message = $"Milestone is evaluated, can not delete returns.",
                });
                return;
            }

            if (tMilestone.Status != (int)TeamMilestoneStatuses.NOT_DONE)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Team milestone's status is not '{TeamMilestoneStatuses.NOT_DONE.ToString()}', can not delete returns.",
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
