using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass
{
    public class GetAllTeamByAssignClassResult : QueryResult
    {
        public PagedList<AllTeamByAssignClassDto>? PaginatedTeams { get; set; }
    }
}
