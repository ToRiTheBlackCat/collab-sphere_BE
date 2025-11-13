using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.TeamFiles.Queries.GetTeamFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Commands.MoveTeamFile
{
    public class MoveTeamFileHandler : CommandHandler<MoveTeamFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MoveTeamFileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(MoveTeamFileCommand request, CancellationToken cancellationToken)
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
                var teamFile = await _unitOfWork.TeamFileRepo.GetById(request.FileId);
                
                // Update Folder path
                teamFile!.FilePathPrefix = request.FilePathPrefix;

                _unitOfWork.TeamFileRepo.Update(teamFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                var folderString = string.IsNullOrEmpty(teamFile.FilePathPrefix) ? "root folder" : $"folder '{teamFile.FilePathPrefix}'";

                result.Message = $"Moved team file '{teamFile.FileName}' ({teamFile.FileId}) to {folderString}.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, MoveTeamFileCommand request)
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
            var moveFile = team.TeamFiles.FirstOrDefault(x => x.FileId == request.FileId);
            if (moveFile == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"No file with ID '{request.FileId}' in team files.",
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

            // Check for duplicated files in destination folder
            var duplicatedFile = team.TeamFiles
                .Any(x =>
                    x.FileId != moveFile.FileId &&
                    x.FileName.Equals(moveFile.FileName, StringComparison.OrdinalIgnoreCase) &&
                    x.FilePathPrefix.Equals(request.FilePathPrefix, StringComparison.OrdinalIgnoreCase)
                );
            if (duplicatedFile)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"Destination folder '{request.FilePathPrefix}' already have a file named '{moveFile.FileName}'.",
                });
                return;
            }
        }
    }
}
