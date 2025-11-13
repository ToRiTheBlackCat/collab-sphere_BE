using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Commands.UpsertPrAnalysis
{
    public class UpsertPrAnalysisCommand : ICommand
    {
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;

        [Required]
        [JsonPropertyName("projectId")]
        public int ProjectId { get; set; }
        [Required]
        [JsonPropertyName("teamId")]
        public int TeamId { get; set; }
        [Required]
        [JsonPropertyName("repositoryId")]
        public long RepositoryId { get; set; }
        //=================================================
        [Required]
        [JsonPropertyName("prNumber")]
        public int PRNumber { get; set; }

        [JsonPropertyName("prTitle")]
        public string? PRTitle { get; set; }

        [JsonPropertyName("prAuthorGithubUsername")]
        public string? PRAuthorGithubUsername { get; set; }
        [JsonPropertyName("prUrl")]
        public string? PRUrl { get; set; }
        [JsonPropertyName("prState")]
        public string? PRState { get; set; }
        [JsonPropertyName("prCreatedAt")]
        public DateTime? PRCreatedAt { get; set; }
        //=================================================
        [JsonPropertyName("aiOverallScore")]
        public int? AIOverallScore { get; set; }
        [JsonPropertyName("aiSummary")]
        public string? AISummary { get; set; }
        [JsonPropertyName("aiDetailedFeedback")]
        public string? AIDetailedFeedback { get; set; }
        [JsonPropertyName("aiBugCount")]
        public int? AIBugCount { get; set; }
        [JsonPropertyName("aiSecurityIssueCount")]
        public int? AISecurityIssueCount { get; set; }
        [JsonPropertyName("aiSuggestionCount")]
        public int? AISuggestionCount { get; set; }
    }
}
