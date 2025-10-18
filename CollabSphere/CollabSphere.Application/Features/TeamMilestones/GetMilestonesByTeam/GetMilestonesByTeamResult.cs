using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamMilestones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.GetMilestonesByTeam
{
    public class GetMilestonesByTeamResult : QueryResult
    {
        public List<TeamMilestoneVM> TeamMilestones { get; set; } = new List<TeamMilestoneVM>();
    }
}
