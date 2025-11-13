using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Queries.GetDetailOfAnalysis
{
    public class GetDetailOfAnalysisHandler : QueryHandler<GetDetailOfAnalysisQuery, GetDetailOfAnalysisResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetDetailOfAnalysisHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<GetDetailOfAnalysisResult> HandleCommand(GetDetailOfAnalysisQuery request, CancellationToken cancellationToken)
        {
            var result = new GetDetailOfAnalysisResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                var foundAnalysis = await _unitOfWork.PrAnalysisRepo.GetById(request.AnalysisId);
                if (foundAnalysis != null)
                {
                    var newDetailAnalysisDto = new DetailAnalysisDto
                    {
                        Id = foundAnalysis.Id,
                        ProjectId = foundAnalysis.ProjectId,
                        TeamId = foundAnalysis.TeamId,
                        RepositoryId = foundAnalysis.RepositoryId,
                        PrTitle = foundAnalysis.PrTitle,
                        PrAuthorGithubUsername = foundAnalysis.PrAuthorGithubUsername,
                        PrUrl = foundAnalysis.PrUrl,
                        PrState = foundAnalysis.PrState,
                        PrCreatedAt = foundAnalysis.PrCreatedAt,
                        AiOverallSCore = foundAnalysis.AiOverallScore,
                        AiSummary = foundAnalysis.AiSummary,
                        AiDetailedFeedback = foundAnalysis.AiDetailedFeedback,
                        AiBugCount = foundAnalysis.AiBugCount,
                        AiSecurityIssueCount = foundAnalysis.AiSecurityIssueCount,
                        AiSuggestionCount = foundAnalysis.AiSuggestionCount,
                        AnalyzeAt = foundAnalysis.AnalyzedAt,

                    };
                    //Map to result
                    result.Analysis = newDetailAnalysisDto;
                    result.IsSuccess = true;
                    result.Message = $"Get detail of Analysis with ID: {request.AnalysisId} successfully";

                    await _unitOfWork.CommitTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = $"Fail to get detail of analysis. Error detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetDetailOfAnalysisQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };
            //Check role permission
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"This role with ID: {request.UserRole} not has permission to get this team details."
                });
                return;
            }
            else
            {
                var foundAnalysis = await _unitOfWork.PrAnalysisRepo.GetById(request.AnalysisId);
                if (foundAnalysis == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.AnalysisId),
                        Message = $"Not found any analysis with ID: {request.AnalysisId}"
                    });
                    return;
                }
            }
        }
    }
}
