using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace CollabSphere.Application.Features.Project.Queries.GetAllProjects
{
    public class GetAllProjectsHandler : QueryHandler<GetAllProjectsQuery, GetAllProjectsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _redis;

        public GetAllProjectsHandler(IUnitOfWork unitOfWork, IDatabase redis)
        {
            _unitOfWork = unitOfWork;
            _redis = redis;
        }

        protected override async Task<GetAllProjectsResult> HandleCommand(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetAllProjectsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                // Filter projects
                var projects = (await _unitOfWork.ProjectRepo.GetAll())
                    .Where(x =>
                        x.Status == ProjectStatuses.APPROVED &&
                        (!request.SubjectIds.Any() || request.SubjectIds.Contains(x.SubjectId)) &&
                        (!request.LecturerIds.Any() || request.LecturerIds.Contains(x.LecturerId)));

                var keyWords = request.Descriptors.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (keyWords.Any())
                {
                    // Weigh projects by name/description keyword match
                    projects = projects.Select(x =>
                    {
                        var name = x.ProjectName.ToLower();
                        var description = x.Description.ToLower();
                        var weight = 0.0;
                        var wordOrderMultipler = 1.0; // Later words has less weight

                        foreach (var kw in keyWords)
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
                    list: projects.Select(x => (ProjectVM)x).ToList(),
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

        private bool ContainsWholeWord(string text, string word)
        {
            return Regex.IsMatch(text, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase);
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetAllProjectsQuery request)
        {
            return;
        }
    }
}
