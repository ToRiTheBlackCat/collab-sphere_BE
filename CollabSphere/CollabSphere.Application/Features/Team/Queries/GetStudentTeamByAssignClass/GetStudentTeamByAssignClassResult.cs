using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetStudentTeamByAssignClass
{
    public class GetStudentTeamByAssignClassResult : QueryResult
    {
        public StudentTeamByAssignClassDto? StudentTeam { get; set; }
    }
}
