using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamMilestones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone
{
    public class CheckTeamMilestoneCommand : ICommand
    {
        public CheckTeamMilestoneDto CheckDto { get; set; }

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
