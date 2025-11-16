using CollabSphere.Application.Base;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Queries.GetTeamWorkspaceByTeam
{
    public class GetTeamWorkspaceByTeamQuery : IQuery<GetTeamWorkspaceByTeamResult>
    {
        [FromRoute(Name = "teamId")]
        public int TeamId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
