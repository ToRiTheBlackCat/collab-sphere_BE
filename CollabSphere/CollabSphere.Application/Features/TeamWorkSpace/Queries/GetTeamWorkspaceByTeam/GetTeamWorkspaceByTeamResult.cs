using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.TeamWorkspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Queries.GetTeamWorkspaceByTeam
{
    public class GetTeamWorkspaceByTeamResult : QueryResult
    {
        public TeamWorkspaceDetailDto? TeamWorkspaceDetail { get; set; }
    }
}
