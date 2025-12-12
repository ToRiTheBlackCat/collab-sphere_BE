using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamMemEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMemberEvaluation.Queries.GetTeamMemberEvaluationsForTeam
{
    public class GetTeamMemberEvaluationsForTeamResult : QueryResult
    {
        public TeamMemEvaluationDto TeamMemEvaluations { get; set; }
    }
}
