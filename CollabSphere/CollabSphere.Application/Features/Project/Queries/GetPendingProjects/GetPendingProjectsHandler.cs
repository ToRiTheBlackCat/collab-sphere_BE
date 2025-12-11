using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Projects;

namespace CollabSphere.Application.Features.Project.Queries.GetPendingProjects
{
    public class GetPendingProjectsHandler : QueryHandler<GetPendingProjectsQuery, GetPendingProjectsResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPendingProjectsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetPendingProjectsResult> HandleCommand(GetPendingProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetPendingProjectsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                var projects = (await _unitOfWork.ProjectRepo.GetAll())
                    .Where(x => x.Status == (int)ProjectStatuses.PENDING);

                var keywords = new HashSet<string>(request.Descriptors.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                if (keywords.Any())
                {
                    // Weigh projects by name/description keyword match
                    projects = projects.Select(x =>
                    {
                        var name = x.ProjectName.ToLower();
                        var description = x.Description.ToLower();
                        var weight = 0.0;
                        var wordOrderMultipler = 1.0; // Later words has less weight

                        foreach (var kw in keywords)
                        {
                            if (name.Contains(kw))
                                weight += 4 * wordOrderMultipler;
                            else if (description.Contains(kw))
                                weight += 1 * wordOrderMultipler;
                            wordOrderMultipler = Math.Max(0.001, wordOrderMultipler - 0.001);
                        }

                        return new
                        {
                            Project = x,
                            Weight = weight
                        };
                    })
                    .Where(x => x.Weight > 0)
                    .OrderByDescending(x => x.Weight)
                    .Select(x => x.Project);
                }

                result.PagedProjects = new Common.PagedList<ProjectVM>(
                    list: projects.ToViewModels(),
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll);

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetPendingProjectsQuery request)
        {
        }
    }
}
