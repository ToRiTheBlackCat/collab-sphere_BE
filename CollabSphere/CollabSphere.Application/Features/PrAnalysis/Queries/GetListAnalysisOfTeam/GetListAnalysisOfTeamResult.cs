using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Queries.GetListOfAnalysis
{
    public class GetListAnalysisOfTeamResult : QueryResult
    {
        public AnalysisDetailDto? AnalysisDetail { get; set; } 
    }

    public class AnalysisDetailDto
    {
        public TeamInfo TeamInfo { get; set; } = new TeamInfo();

        public RepositoryInfo RepositoryInfo { get; set; } = new RepositoryInfo();
        public PaginationDto Pagination { get; set; } = new PaginationDto();

    }
    public class TeamInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RepositoryInfo
    {
        public long Id { get; set; }
        public string FullName { get; set; }
    }

    public class PaginationDto
    {
        public bool IsSuccess { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }
    public class Item
    {
        public int AnalysisId { get; set; }
        public int PrNumber { get; set; }
        public string? PrTitle { get; set; }
        public string? PrAuthor { get; set; }
        public string? PrUrl { get; set; }
        public int? AiScore { get; set; }
        public int? BugCount { get; set; }
        public int? SecurityIssueCount { get; set; }
        public DateTime? AnalyzedAt { get; set; }
    }
}
