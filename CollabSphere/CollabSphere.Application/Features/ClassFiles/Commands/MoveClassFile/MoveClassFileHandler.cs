using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.ClassFiles.Commands.MoveClassFile
{
    public class MoveClassFileHandler : CommandHandler<MoveClassFileCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MoveClassFileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(MoveClassFileCommand request, CancellationToken cancellationToken)
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
                // Get class file entity
                var newClassFile = await _unitOfWork.ClassFileRepo.GetById(request.FileId);

                // Update field
                newClassFile!.FilePathPrefix = request.FilePathPrefix;

                _unitOfWork.ClassFileRepo.Update(newClassFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                var folderString = newClassFile.FilePathPrefix.Equals("/") ? "Root folder" : $"folder '{newClassFile.FilePathPrefix}'";

                result.Message = $"Moved class file '{newClassFile.FileName}' ({newClassFile.FileId}) to {folderString}.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                await _unitOfWork.RollbackTransactionAsync();
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, MoveClassFileCommand request)
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
            // Only move file from class in request
            else if (classFile.ClassId != request.ClassId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FileId),
                    Message = $"No file with ID '{request.FileId}' in class '{classEntity.ClassName}'({classEntity.ClassId}).",
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
                x.FileId != classFile.FileId &&
                x.FileName.Equals(classFile.FileName, StringComparison.OrdinalIgnoreCase) &&
                x.FilePathPrefix.Equals(request.FilePathPrefix, StringComparison.OrdinalIgnoreCase)
            );
            if (duplicatedFile)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.FilePathPrefix),
                    Message = $"Destination folder '{request.FilePathPrefix}' already have a file named '{classFile.FileName}'.",
                });
                return;
            }
        }
    }
}
