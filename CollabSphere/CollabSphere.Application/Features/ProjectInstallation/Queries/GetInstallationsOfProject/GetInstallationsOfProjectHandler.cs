using Amazon.S3.Model.Internal.MarshallTransformations;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ProjectRepo;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using CollabSphere.Application.Mappings.ProjectRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject
{
    public class GetInstallationsOfProjectHandler : QueryHandler<GetInstallationsOfProjectQuery, GetInstallationsOfProjectResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetInstallationsOfProjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetInstallationsOfProjectResult> HandleCommand(GetInstallationsOfProjectQuery request, CancellationToken cancellationToken)
        {
            var result = new GetInstallationsOfProjectResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var installations = await _unitOfWork.ProjectInstallationRepo.SearchInstallationsOfProject(request.ProjectId, request.ConnectedUserId, request.FromDate, request.IsDesc);

                if (installations != null || installations.Count() > 0)
                {
                    var mappedInstalls = installations.ListInstallations_To_ListAllInstallationsOfProjectDto();

                    result.PaginatedInstalls = new PagedList<AllInstallationsOfProjectDto>(
                    list: mappedInstalls,
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll
                    );

                    result.IsSuccess = true;
                    result.Message = $"Get installations of project with ID: {request.ProjectId} sucessfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetInstallationsOfProjectQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check role permission
            if (bypassRoles.Contains(request.UserRole))
            {
                var foundProject = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
                if (foundProject == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.ProjectId),
                        Message = $"Not found any project with that Id: {request.ProjectId}"
                    });
                    return;
                }
                if (request.ConnectedUserId != 0)
                {
                    var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.ConnectedUserId);

                    if (foundUser == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.ConnectedUserId),
                            Message = $"Not found any project with that Id: {request.ProjectId}"
                        });
                        return;
                    }
                }
            }
            else
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserRole),
                    Message = $"Your role do not have permisison to use this function"
                });
                return;
            }
        }
    }
}
