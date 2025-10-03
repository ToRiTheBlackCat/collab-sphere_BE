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
                        x.Status == ProjectStatus.APPROVED &&
                        (!request.SubjectIds.Any() || request.SubjectIds.Contains(x.SubjectId)) &&
                        (!request.LecturerIds.Any() || request.LecturerIds.Contains(x.LecturerId)));

                var keyWords = request.Descriptors.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (keyWords.Any())
                {
                    projects = projects
                        .Select(x => new
                        {
                            Project = x,
                            Score = 4 * keyWords.Count(k => ContainsWholeWord(x.ProjectName.ToLower(), k)) +
                                keyWords.Count(k => ContainsWholeWord(x.Description.ToLower(), k))
                        })
                        .Where(x => x.Score > 0)
                        .OrderByDescending(x => x.Score)
                            .ThenBy(x => x.Project.ProjectName)
                        .Select(x => x.Project);
                }

                result.Projects = projects.Select(x => (ProjectVM)x).ToList();
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
