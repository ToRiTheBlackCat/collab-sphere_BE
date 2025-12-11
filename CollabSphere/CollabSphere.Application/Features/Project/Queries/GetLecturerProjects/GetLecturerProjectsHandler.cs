using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.Projects;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Project.Queries.GetTeacherProjects
{
    public class GetLecturerProjectsHandler : QueryHandler<GetLecturerProjectsQuery, GetLecturerProjectsResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLecturerProjectsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetLecturerProjectsResult> HandleCommand(GetLecturerProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetLecturerProjectsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                var keywords = new HashSet<string>(request.Descriptors.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                var projects = (await _unitOfWork.ProjectRepo.GetAll())
                    // Filter projects
                    .Where(x =>
                        x.LecturerId == request.LecturerId &&
                        (!request.Statuses.Any() || request.Statuses.Contains(x.Status)))
                    .ToList();

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
                    .Select(x => x.Project)
                    .ToList();
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

        protected override async Task ValidateRequest(List<OperationError> errors, GetLecturerProjectsQuery request)
        {
            return;
        }
    }
}
