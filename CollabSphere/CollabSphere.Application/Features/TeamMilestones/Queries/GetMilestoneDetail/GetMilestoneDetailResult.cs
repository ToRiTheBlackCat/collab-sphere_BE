using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamMilestones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail
{
    public class GetMilestoneDetailResult : QueryResult
    {
        public TeamMilestoneDetailDto? TeamMilestone { get; set; }
    }
}
