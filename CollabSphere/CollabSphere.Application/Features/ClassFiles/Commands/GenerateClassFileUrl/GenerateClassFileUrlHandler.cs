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

namespace CollabSphere.Application.Features.ClassFiles.Commands.GenerateClassFileUrl
{
    public class GenerateClassFileUrlHandler : CommandHandler<GenerateClassFileUrlCommand, GenerateClassFileUrlResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3 _s3Client;

        public GenerateClassFileUrlHandler(IUnitOfWork unitOfWork, IAmazonS3 s3Client)
        {
            _unitOfWork = unitOfWork;
            _s3Client = s3Client;
        }

        protected override async Task<GenerateClassFileUrlResult> HandleCommand(GenerateClassFileUrlCommand request, CancellationToken cancellationToken)
        {
            var result = new GenerateClassFileUrlResult()
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
                var classFile = await _unitOfWork.ClassFileRepo.GetById(request.FileId);

                // Get new pre-signed URL
                var currentTime = DateTime.UtcNow;
                var urlResponse = await _s3Client.GetPresignedUrlFromS3Async(classFile!.ObjectKey, currentTime);

                // Update database entry
                classFile.FileUrl = urlResponse.url;
                classFile.UrlExpireTime = urlResponse.expireTime;

                _unitOfWork.ClassFileRepo.Update(classFile);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Return updated milestone file
                result.FileUrl = classFile.FileUrl;
                result.UrlExpireTime = classFile.UrlExpireTime;
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

        protected override async Task ValidateRequest(List<OperationError> errors, GenerateClassFileUrlCommand request)
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

            // Requester is a Lecturer
            if (request.UserRole == RoleConstants.LECTURER)
            {
                // Check if lecturer is assigned to class
                if (request.UserId != classEntity.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{classEntity.ClassId}'.",
                    });
                    return;
                }
            }
            // Requester is a Student
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                // Check if is member of class
                var isClassMember = classEntity.ClassMembers
                    .Select(cm => cm.StudentId)
                    .ToHashSet()
                    .Contains(request.UserId);
                if (!isClassMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a class member of the class with ID '{classEntity.ClassId}'.",
                    });
                    return;
                }
            }

            // Check file
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
            else if (classFile.ClassId != classEntity.ClassId)
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
