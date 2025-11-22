using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Queries.GetDetailOfAnalysis
{
    public class GetDetailOfAnalysisResult : QueryResult
    {
        public DetailAnalysisDto? Analysis { get; set; }
    }

    public class DetailAnalysisDto
    {
        public long Id { get; set; }
        public int ProjectId { get; set; }
        public int TeamId { get; set; }
        public long RepositoryId { get; set; }
        public string? PrTitle { get; set; }
        public string? PrAuthorGithubUsername { get; set; }
        public string? PrUrl { get; set; }
        public string? PrState { get; set; }
        public DateTime? PrCreatedAt { get; set; }
        public int? AiOverallSCore { get; set; }
        public string? AiSummary { get; set; }
        public string? AiDetailedFeedback { get; set; }
        public int? AiBugCount { get; set; }
        public int? AiSecurityIssueCount { get; set; }
        public int? AiSuggestionCount { get; set; }
        public DateTime? AnalyzeAt { get; set; }
    }
}
