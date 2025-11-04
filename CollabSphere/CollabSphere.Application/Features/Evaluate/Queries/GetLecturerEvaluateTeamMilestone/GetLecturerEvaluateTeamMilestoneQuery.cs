using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluateTeamMilestone
{
    public class GetLecturerEvaluateTeamMilestoneQuery : IQuery<GetLecturerEvaluateTeamMilestoneResult>
    {
        [FromRoute(Name = "teamMilestoneId")]
        public int TeamMilestoneId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
