using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class UpdateTeamMilestoneDto
    {
        public int TeamMilestoneId = -1;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        [Length(3, 150)]
        public string? Tile { get; set; }

        [Length(3, 700)]
        public string? Description { get; set; }
    }
}
