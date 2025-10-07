using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetProjectsOfClass
{
    public class GetProjectsOfClassHandler : QueryHandler<GetProjectsOfClassQuery, GetProjectsOfClassResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProjectsOfClassHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetProjectsOfClassResult> HandleCommand(GetProjectsOfClassQuery request, CancellationToken cancellationToken)
        {
            var result = new GetProjectsOfClassResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var keywords = new HashSet<string>(request.Descriptors.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                var projectAssignments = await _unitOfWork.ProjectAssignmentRepo.GetProjectAssignmentsByClassAsync(request.ClassId);
                var projects = projectAssignments.Select(x => x.Project);

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

                // View all

                var pagedList = new PagedList<ProjectVM>(
                    list: projects.Select(x => (ProjectVM)x),
                    pageNum: request.PageNum,
                    pageSize: request.PageSize,
                    viewAll: request.ViewAll);
                result.PagedProjects = pagedList;

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetProjectsOfClassQuery request)
        {
            return;
        }
    }
}
