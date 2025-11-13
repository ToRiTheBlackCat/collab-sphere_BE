using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ProjectRepo;
using CollabSphere.Application.DTOs.Validation;

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
                Message = string.Empty,
                Installations = new List<AllReposOfProjectDto>()
            };

            try
            {
                var installations = await _unitOfWork.ProjectRepoMappingRepo.SearchInstallationsOfProject(request.ProjectId, request.ConnectedUserId, request.TeamId, request.FromDate, request.IsDesc);

                if (installations != null && installations.Any())
                {
                    foreach (var teamGroup in installations)
                    {
                        var firstItem = teamGroup.Value.FirstOrDefault();
                        if (firstItem == null) continue; // Skip  team if its repo list is empty

                        var teamDto = new AllReposOfProjectDto
                        {
                            TeamId = teamGroup.Key,
                            TeamName = firstItem.Team.TeamName,

                            Repositories = teamGroup.Value.Select(repo => new RepoResponseDto
                            {
                                RepositoryFullName = repo.RepositoryFullName,
                                RepositoryId = repo.RepositoryId
                            }).ToList()
                        };

                        result.Installations.Add(teamDto);
                    }

                    result.IsSuccess = true;
                    result.Message = $"Get installations of project with ID: {request.ProjectId} successfully";
                }
                else
                {
                    result.Message = $"No installations found for project with ID: {request.ProjectId}";
                    result.IsSuccess = true; 
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

                if (request.TeamId != 0)
                {
                    var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);

                    if (foundTeam == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.TeamId),
                            Message = $"Not found any team with that Id: {request.TeamId}"
                        });
                        return;
                    }
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
