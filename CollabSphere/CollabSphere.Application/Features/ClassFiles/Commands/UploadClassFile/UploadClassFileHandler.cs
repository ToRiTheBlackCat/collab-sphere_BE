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

namespace CollabSphere.Application.Features.ClassFiles.Commands.UploadClassFile
{
    public class UploadClassFileHandler : CommandHandler<UploadClassFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public UploadClassFileHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(UploadClassFileCommand request, CancellationToken cancellationToken)
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
                    AwsS3HelperPaths.Class,
                    request.ClassId,
                    null
                );

                // Create database entry
                var newClassFile = new ClassFile()
                {
                    ClassId = request.ClassId,
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

                await _unitOfWork.ClassFileRepo.Create(newClassFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                var folderString = newClassFile.FilePathPrefix.Equals("/") ? "Root folder" : $"folder '{newClassFile.FilePathPrefix}'";
                result.Message = $"Uploaded file '{newClassFile.FileName}' ({newClassFile.FileId}) to class with ID '{newClassFile.ClassId}'.\n At {folderString}.";
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

        protected override async Task ValidateRequest(List<OperationError> errors, UploadClassFileCommand request)
        {
            // Check class
            var classEntity = await _unitOfWork.ClassRepo.GetClassDetail(request.ClassId);
            if (classEntity == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ClassId),
                    Message = $"No class with ID '{request.ClassId}'.",
                });
                return;
            }

            // Check lecturer
            if (request.UserId != classEntity.LecturerId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{classEntity.ClassId}'.",
                });
                return;
            }

            // Check file
            if (!FileValidator.ValidateFile(request.File, out var errorMessage))
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.UserId),
                    Message = errorMessage,
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

            // Check for duplicated files in class (same name & path prefix)
            request.FilePathPrefix = string.IsNullOrWhiteSpace(request.FilePathPrefix) ? "/" : request.FilePathPrefix.Trim();

            var existClassFiles = await _unitOfWork.ClassFileRepo.GetFilesByClass(request.ClassId);
            var duplicatedFile = existClassFiles.Any(x =>
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
