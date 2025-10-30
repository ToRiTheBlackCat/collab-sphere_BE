using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone
{
    public class CreateCustomTeamMilestoneResult : CommandResult
    {
        public int TeamMilestoneId { get; set; } = -1;
    }
}
