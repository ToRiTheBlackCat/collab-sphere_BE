using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ClassFiles.Commands.DeleteClassFile
{
    public class DeleteClassFileHandler : CommandHandler<DeleteClassFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public DeleteClassFileHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteClassFileCommand request, CancellationToken cancellationToken)
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
                // Get class file 
                var cFile = (await _unitOfWork.ClassFileRepo.GetById(request.FileId))!;

                // Delete file from Aws S3
                await _s3Client.DeleteFileFromS3Async(cFile.ObjectKey);

                // Delete database entry
                _unitOfWork.ClassFileRepo.Delete(cFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted file '{cFile.FileName}' ({cFile.FileId}) from class with ID '{cFile.ClassId}'.";
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

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteClassFileCommand request)
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

            // Check file existence
            var classFile = await _unitOfWork.ClassFileRepo.GetById(request.FileId);
            if (classFile == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"No class file with ID '{request.FileId}'.",
                });
                return;
            }
            // Only delete file from class in request
            else if (classFile.ClassId != request.ClassId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"No file with ID '{request.FileId}' in class '{classEntity.ClassName}'({classEntity.ClassId}).",
                });
                return;
            }
        }
    }
}
