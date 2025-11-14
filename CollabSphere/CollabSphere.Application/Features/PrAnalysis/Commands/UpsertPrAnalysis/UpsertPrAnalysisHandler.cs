using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Commands.UpsertPrAnalysis
{
    public class UpsertPrAnalysisHandler : CommandHandler<UpsertPrAnalysisCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpsertPrAnalysisHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(UpsertPrAnalysisCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundPrAnalysis = await _unitOfWork.PrAnalysisRepo.SearchPrAnalysis(request.ProjectId, request.TeamId, request.RepositoryId, request.PRNumber);
                if (foundPrAnalysis != null)
                {
                    #region Update found Analysis
                    if (!string.IsNullOrEmpty(request.PRTitle))
                    {
                        foundPrAnalysis.PrTitle = request.PRTitle;
                    }
                    if (!string.IsNullOrEmpty(request.PRAuthorGithubUsername))
                    {
                        foundPrAnalysis.PrAuthorGithubUsername = request.PRAuthorGithubUsername;
                    }
                    if (!string.IsNullOrEmpty(request.PRUrl))
                    {
                        foundPrAnalysis.PrUrl = request.PRUrl;
                    }
                    if (!string.IsNullOrEmpty(request.PRState))
                    {
                        foundPrAnalysis.PrState = request.PRState;
                    }
                    if (request.PRCreatedAt.HasValue)
                    {
                        foundPrAnalysis.PrCreatedAt = request.PRCreatedAt;
                    }
                    if (request.AIOverallScore != 0)
                    {
                        foundPrAnalysis.AiOverallScore = request.AIOverallScore;
                    }
                    if (!string.IsNullOrEmpty(request.AISummary))
                    {
                        foundPrAnalysis.AiSummary = request.AISummary;
                    }
                    if (!string.IsNullOrEmpty(request.AIDetailedFeedback))
                    {
                        foundPrAnalysis.AiDetailedFeedback = request.AIDetailedFeedback;
                    }
                    if (request.AIBugCount != 0)
                    {
                        foundPrAnalysis.AiBugCount = request.AIBugCount;
                    }
                    if (request.AISecurityIssueCount != 0)
                    {
                        foundPrAnalysis.AiSecurityIssueCount = request.AISecurityIssueCount;
                    }
                    if (request.AISuggestionCount != 0)
                    {
                        foundPrAnalysis.AiSuggestionCount = request.AISuggestionCount;
                    }
                    foundPrAnalysis.AnalyzedAt = DateTime.UtcNow;

                    _unitOfWork.PrAnalysisRepo.Update(foundPrAnalysis);
                    await _unitOfWork.SaveChangesAsync();

                    result.Message = $"Update analysis with projectId: {request.ProjectId} | teamId: {request.TeamId} | repositoryId: {request.RepositoryId} | PrNumber: {request.PRNumber} sucessfully";
                    #endregion
                }
                else
                {
                    #region Create NewAnalysis
                    var newPrAnalysis = new Domain.Entities.PrAnalysis
                    {
                        ProjectId = request.ProjectId,
                        TeamId = request.TeamId,
                        RepositoryId = request.RepositoryId,
                        PrNumber = request.PRNumber,
                        AnalyzedAt = DateTime.UtcNow,
                    };
                    if (!string.IsNullOrEmpty(request.PRTitle))
                    {
                        newPrAnalysis.PrTitle = request.PRTitle;
                    }
                    if (!string.IsNullOrEmpty(request.PRAuthorGithubUsername))
                    {
                        newPrAnalysis.PrAuthorGithubUsername = request.PRAuthorGithubUsername;
                    }
                    if (!string.IsNullOrEmpty(request.PRUrl))
                    {
                        newPrAnalysis.PrUrl = request.PRUrl;
                    }
                    if (!string.IsNullOrEmpty(request.PRState))
                    {
                        newPrAnalysis.PrState = request.PRState;
                    }
                    if (request.PRCreatedAt.HasValue)
                    {
                        newPrAnalysis.PrCreatedAt = request.PRCreatedAt;
                    }
                    if (request.AIOverallScore != 0)
                    {
                        newPrAnalysis.AiOverallScore = request.AIOverallScore;
                    }
                    if (!string.IsNullOrEmpty(request.AISummary))
                    {
                        newPrAnalysis.AiSummary = request.AISummary;
                    }
                    if (!string.IsNullOrEmpty(request.AIDetailedFeedback))
                    {
                        newPrAnalysis.AiDetailedFeedback = request.AIDetailedFeedback;
                    }
                    if (request.AIBugCount != 0)
                    {
                        newPrAnalysis.AiBugCount = request.AIBugCount;
                    }
                    if (request.AISecurityIssueCount != 0)
                    {
                        newPrAnalysis.AiSecurityIssueCount = request.AISecurityIssueCount;
                    }
                    if (request.AISuggestionCount != 0)
                    {
                        newPrAnalysis.AiSuggestionCount = request.AISuggestionCount;
                    }

                    await _unitOfWork.PrAnalysisRepo.Create(newPrAnalysis);
                    await _unitOfWork.SaveChangesAsync();

                    result.Message = $"Create analysis with projectId: {request.ProjectId} | teamId: {request.TeamId} | repositoryId: {request.RepositoryId} | PrNumber: {request.PRNumber} sucessfully";
                    #endregion
                }

                await _unitOfWork.CommitTransactionAsync();
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = $"Fail to Create|Update analysis with projectId: {request.ProjectId} | teamId: {request.TeamId} | repositoryId: {request.RepositoryId} | PrNumber: {request.PRNumber}. Error detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpsertPrAnalysisCommand request)
        {
            //Find project
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

            //Find team
            var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
            if (foundTeam == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ProjectId),
                    Message = $"Not found any project with that Id: {request.TeamId}"
                });
                return;
            }
        }
    }
}
