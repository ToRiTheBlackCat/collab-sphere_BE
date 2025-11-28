using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamMilestones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone
{
    public class CreateCustomTeamMilestoneCommand : ICommand<CreateCustomTeamMilestoneResult>
    {
        public CreateTeamMilestoneDto MilestoneDto { get; set; } = default!;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
