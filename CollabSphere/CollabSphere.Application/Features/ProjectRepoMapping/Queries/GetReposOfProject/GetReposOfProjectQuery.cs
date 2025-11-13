using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject
{
    public class GetReposOfProjectQuery :  IQuery<GetReposOfProjectResult>
    {
        [FromRoute(Name = "projectId")]
        public int ProjectId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;

        [FromQuery]
        [JsonPropertyName("teamId")]
        public int TeamId { get; set; } = 0;
        [FromQuery]
        [JsonPropertyName("connectedUserId")]
        public int ConnectedUserId { get; set; } = 0;
        [FromQuery]
        [JsonPropertyName("fromDate")]
        public DateTime? FromDate { get; set; }
        [FromQuery]
        [JsonPropertyName("isDesc")]
        public bool IsDesc { get; set; } = false;
    }
}

