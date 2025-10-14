using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent
{
    public class GetAllTeamOfStudentResult : QueryResult
    {
        public PagedList<AllTeamOfStudentDto>? PaginatedTeams { get; set; }
    }
}
