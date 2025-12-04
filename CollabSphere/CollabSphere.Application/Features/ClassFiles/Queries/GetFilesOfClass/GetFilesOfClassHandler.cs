using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.TeamFiles;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.ClassFiles;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.ClassFiles.Queries.GetFilesOfClass
{
    public class GetFilesOfClassHandler : QueryHandler<GetFilesOfClassQuery, GetFilesOfClassResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetFilesOfClassHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetFilesOfClassResult> HandleCommand(GetFilesOfClassQuery request, CancellationToken cancellationToken)
        {
            var result = new GetFilesOfClassResult();

            try
            {
                // Get files in class
                var classFiles = await _unitOfWork.ClassFileRepo.GetFilesByClass(request.ClassId);

                // Generate img URL for lecturer avatar
                var imgDictionary = new Dictionary<int, string>(); // Mapping LecturerId - AvatarImgUrl (In case class change lecturer)
                foreach (var classFile in classFiles)
                {
                    var lecturerId = classFile.User.Lecturer.LecturerId;
                    if (imgDictionary.TryGetValue(lecturerId, out var avatarImgUrl))
                    {
                        classFile.User.Lecturer.AvatarImg = avatarImgUrl;
                    }
                    else
                    {
                        var generatedUrl = await _cloudinaryService.GetImageUrl(classFile.User.Lecturer.AvatarImg);
                        classFile.User.Lecturer.AvatarImg = generatedUrl;
                        imgDictionary.Add(lecturerId, generatedUrl);
                    }
                }

                result.Grouping = classFiles.ToViewModels()
                    .GroupBy(x => x.FilePathPrefix)
                    .ToDictionary(x => x.Key);
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetFilesOfClassQuery request)
        {
            // Check class exist
            var classEntity = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (classEntity == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ClassId),
                    Message = $"No class with ID '{request.ClassId}' found.",
                });
                return;
            }

            // Check viewing user when is Lecturer
            if (request.UserId == RoleConstants.LECTURER)
            {
                // Lecturer has to be assigned to class
                if (classEntity.LecturerId != request.UserId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.ClassId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of class '{classEntity.ClassName}'({classEntity.ClassId}).",
                    });
                }
            }
            // Check viewing user when is Student
            else if (request.UserId == RoleConstants.STUDENT)
            {
                // Student has to be member of class
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
                }
            }
        }
    }
}
