using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class CheckTeamMilestoneDto
    {
        [FromRoute(Name = "teamMilestoneId")]
        public int TeamMilestoneId { get; set; } = -1;

        [FromQuery]
        public bool IsDone { get; set; } = true;
    }
}
