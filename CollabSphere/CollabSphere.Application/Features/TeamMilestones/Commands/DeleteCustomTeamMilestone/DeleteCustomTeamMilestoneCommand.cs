using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Commands.DeleteCustomTeamMilestone
{
    public class DeleteCustomTeamMilestoneCommand : ICommand
    {
        public int TeamMilestoneId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
