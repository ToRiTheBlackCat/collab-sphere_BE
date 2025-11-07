using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Evaluate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluateTeamMilestone
{
    public class GetLecturerEvaluateTeamMilestoneResult: QueryResult
    {
        public GetLecturerEvaluateTeamMilestoneDto? LecturerEvaluateTeamMilestone { get; set; }
    }
}
