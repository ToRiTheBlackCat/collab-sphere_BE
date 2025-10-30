using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Evaluate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetOwnEvaluationsForOtherInTeam
{
    public class GetOwnEvaluationsForOtherInTeamResult : QueryResult
    {
        public List<GetOwnEvaluationsForOtherInTeamDto> OwnEvaluations { get; set; } = new List<GetOwnEvaluationsForOtherInTeamDto>();
    }
}
