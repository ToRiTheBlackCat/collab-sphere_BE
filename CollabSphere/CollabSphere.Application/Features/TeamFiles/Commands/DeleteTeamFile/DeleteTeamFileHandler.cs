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

namespace CollabSphere.Application.Features.TeamFiles.Commands.DeleteTeamFile
{
    public class DeleteTeamFileHandler : CommandHandler<DeleteTeamFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public DeleteTeamFileHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteTeamFileCommand request, CancellationToken cancellationToken)
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
                // Get team file 
                var teamFile = (await _unitOfWork.TeamFileRepo.GetById(request.FileId))!;

                // Delete file from Aws S3
                await _s3Client.DeleteFileFromS3Async(teamFile.ObjectKey);

                // Delete database entry
                _unitOfWork.TeamFileRepo.Delete(teamFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted file '{teamFile.FileName}' ({teamFile.FileId}) from team with ID '{request.TeamId}'.";
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

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteTeamFileCommand request)
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
