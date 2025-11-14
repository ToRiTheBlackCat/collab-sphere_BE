using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdHandler : QueryHandler<GetClassByIdQuery, GetClassByIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public GetClassByIdHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetClassByIdResult> HandleCommand(GetClassByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetClassByIdResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                Authorized = true,
            };

            try
            {
                var classEntity = await _unitOfWork.ClassRepo.GetById(request.ClassId);
                if (classEntity != null)
                {
                    // Viewer is a Student but NOT in Class
                    if (request.ViewerRole == RoleConstants.STUDENT && !classEntity.ClassMembers.Any(x => x.StudentId == request.ViewerUId))
                    {
                        result.Authorized = false;
                        result.Message = "You are not a Student in this Class.";
                    }
                    else if (request.ViewerRole == RoleConstants.LECTURER && classEntity.LecturerId != request.ViewerUId)
                    {
                        result.Authorized = false;
                        result.Message = "You are not the assigned lecturer of the Class.";
                    }
                    else
                    {
                        // Generate Class File's Lecturer Avatar Img URL
                        foreach (var classFile in classEntity.ClassFiles)
                        {
                            var url = await _cloudinaryService.GetImageUrl(classFile.User.Lecturer.AvatarImg);
                            classFile.User.Lecturer.AvatarImg = url;
                        }

                        // Generate Class Members' User Avatar Img URL
                        foreach (var member in classEntity.ClassMembers)
                        {
                            var url = await _cloudinaryService.GetImageUrl(member.Student.AvatarImg);
                            member.Student.AvatarImg = url;
                        }

                        // Generate teams' Avatar Img URL
                        foreach (var team in classEntity.Teams)
                        {
                            var url = await _cloudinaryService.GetImageUrl(team.TeamImage);
                            team.TeamImage = url;
                        }

                        // Show enrol key if viewer is not Student Role
                        var showEnrolKey = request.ViewerRole != RoleConstants.STUDENT;

                        result.Class = classEntity.ToDetailDto(showEnrolKey);
                    }
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetClassByIdQuery request)
        {
            return;
        }
    }
}
