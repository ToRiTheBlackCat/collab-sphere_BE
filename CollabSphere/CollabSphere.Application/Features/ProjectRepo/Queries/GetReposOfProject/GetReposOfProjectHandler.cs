using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
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
    public class GetReposOfProjectHandler : QueryHandler<GetReposOfProjectQuery, GetReposOfProjectResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetReposOfProjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetReposOfProjectResult> HandleCommand(GetReposOfProjectQuery request, CancellationToken cancellationToken)
        {
            var result = new GetReposOfProjectResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var repos = await _unitOfWork.ProjectRepo_Repo.SearchReposOfProject(request.ProjectId, request.RepositoryName, request.RepositoryUrl, request.ConnectedUserId, request.FromDate, request.IsDesc);

                if (repos != null || repos.Count() > 0)
                {
                    var mappedRepos = repos.ListRepo_To_ListAllReposOfProjectDto();

                    result.PaginatedRepos = new PagedList<AllReposOfProjectDto>(
                    list: mappedRepos,
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll
                    );

                    result.IsSuccess = true;
                    result.Message = $"Get repo of project with ID: {request.ProjectId} sucessfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetReposOfProjectQuery request)
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
    }
}
