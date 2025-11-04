using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Evaluate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetOtherEvaluationsForOwnInTeam
{
    public class GetOwnEvaluationsForOtherInTeamResult : QueryResult
    {
        public List<OtherEvaluationsForOwnInTeamDto> OtherEvaluations { get; set; } = new List<OtherEvaluationsForOwnInTeamDto>();
    }
}
