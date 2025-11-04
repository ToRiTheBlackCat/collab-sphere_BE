using CollabSphere.Application.Base;
using CollabSphere.Application.Features.Evaluate.Queries.GetOtherEvaluationsForOwnInTeam;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetOwnEvaluationsForOtherInTeam
{
    public class GetOwnEvaluationsForOtherInTeamQuery : IQuery<GetOwnEvaluationsForOtherInTeamResult>
    {
        [FromRoute(Name = "teamId")]
        public int TeamId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
