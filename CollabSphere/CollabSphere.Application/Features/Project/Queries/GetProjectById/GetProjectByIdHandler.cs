using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetProjectById
{
    public class GetProjectByIdHandler : QueryHandler<GetProjectByIdQuery, GetProjectByIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProjectByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetProjectByIdResult> HandleCommand(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetProjectByIdResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                IsAuthorized = true,
            };

            try
            {
                var project = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
                if (project != null)
                {
                    result.Project = (ProjectVM)project;

                    // Check viewing privileges if Project is NOT APPROVED & User is NOT HEAD_DEPARTMENT
                    if (project.Status != ProjectStatuses.APPROVED && request.UserRole != RoleConstants.HEAD_DEPARTMENT)
                    {
                        var lecturer = await _unitOfWork.LecturerRepo.GetById(request.UserId);
                        if (lecturer == null || project.LecturerId != lecturer.LecturerId) // Is not owner
                        {
                            result.IsAuthorized = false;
                            result.Message = "You are not authorized to view this Project.";
                        }
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

        protected override async Task ValidateRequest(List<OperationError> errors, GetProjectByIdQuery request)
        {
            return;
        }
    }
}
