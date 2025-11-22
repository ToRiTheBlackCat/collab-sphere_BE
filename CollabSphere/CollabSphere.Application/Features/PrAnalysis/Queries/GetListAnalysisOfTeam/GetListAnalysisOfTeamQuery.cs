using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Queries.GetListOfAnalysis
{
    public class GetListAnalysisOfTeamQuery : IQuery<GetListAnalysisOfTeamResult>
    {
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;

        [Required]
        [FromRoute(Name = "teamId")]
        public int TeamId { get; set; } = 0;

        [Required]
        [FromQuery]
        [JsonPropertyName("repositoryId")]
        public long RepositoryId { get; set; } = 0;

        [FromQuery]
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; } = 1;
        [FromQuery]
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 10;
        [FromQuery]
        [JsonPropertyName("ssDesc")]
        public bool IsDesc { get; set; } = false;
    }
}
