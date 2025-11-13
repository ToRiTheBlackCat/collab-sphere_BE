using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Commands.UploadTeamFile
{
    public class UploadTeamFileHandler : CommandHandler<UploadTeamFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public UploadTeamFileHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(UploadTeamFileCommand request, CancellationToken cancellationToken)
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
                    AwsS3HelperPaths.Team,
                    request.TeamId,
                    null
                );

                // Create DB entry
                var teamFile = new TeamFile()
                {
                    TeamId = request.TeamId,
                    UserId = request.UserId,
                    FileName = uploadResponse.FileName,
                    FilePathPrefix = request.FilePathPrefix,
                    Type = request.File.ContentType,
                    FileSize = request.File.Length,
                    CreatedAt = currentTime,
                    ObjectKey = uploadResponse.ObjectKey,
                    FileUrl = uploadResponse.PresignedUrl,
                    UrlExpireTime = uploadResponse.UrlExpireTime,
                };

                await _unitOfWork.TeamFileRepo.Create(teamFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                var folderString = string.IsNullOrEmpty(request.FilePathPrefix) ? "root folder" : $"folder '{request.FilePathPrefix}'";
                result.Message = $"Uploaded file '{teamFile.FileName}' ({teamFile.FileId}) to team with ID '{request.TeamId}'. \nAt {folderString}.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UploadTeamFileCommand request)
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

            // Validate file
            if (!FileValidator.ValidateFile(request.File, out var fileError, 15))
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.File),
                    Message = fileError,
                });
                return;
            }

            // Validate prefix
            if (!FileValidator.IsValidPrefix(request.FilePathPrefix, out var pathError))
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FilePathPrefix),
                    Message = pathError,
                });
                return;
            }

            // Check for duplicated files
            var duplicatedFile = team.TeamFiles
                .Any(x =>
                    x.FileName.Equals(request.File.FileName, StringComparison.OrdinalIgnoreCase) &&
                    x.FilePathPrefix.Equals(request.FilePathPrefix, StringComparison.OrdinalIgnoreCase)
                );
            if (duplicatedFile)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.File),
                    Message = $"Destination folder '{request.FilePathPrefix}' already have a file named '{request.File.FileName}'.",
                });
                return;
            }
        }
    }
}
