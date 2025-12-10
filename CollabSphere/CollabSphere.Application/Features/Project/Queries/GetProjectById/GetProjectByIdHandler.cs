using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Projects;
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
                var project = await _unitOfWork.ProjectRepo.GetProjectDetail(request.ProjectId);
                if (project != null)
                {
                    result.Project = project.ToViewModel();

                    // Roles that bypass viewing privileges
                    var bypassRoles = new int[] { RoleConstants.HEAD_DEPARTMENT, RoleConstants.ADMIN, RoleConstants.STAFF };

                    // Check viewing privileges if Project is NOT APPROVED & User is NOT HEAD_DEPARTMENT
                    if (project.Status != (int)ProjectStatuses.APPROVED && !bypassRoles.Contains(request.UserRole))
                    {
                        if (request.UserRole != RoleConstants.STUDENT)
                        {
                            var lecturer = await _unitOfWork.LecturerRepo.GetById(request.UserId);
                            if (lecturer == null || project.LecturerId != lecturer.LecturerId) // Is not owner
                            {
                                result.IsAuthorized = false;
                            }
                        }
                        else
                        {
                            result.IsAuthorized = false;
                        }
                    }
                }

                result.Message = !result.IsAuthorized ? "You are not authorized to view this Project." : string.Empty;
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
