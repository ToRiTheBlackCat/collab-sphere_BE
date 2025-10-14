using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Teams;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass
{
    public class GetAllTeamByAssignClassResult : QueryResult
    {
        public PagedList<AllTeamByAssignClassDto>? PaginatedTeams { get; set; }
    }
}
