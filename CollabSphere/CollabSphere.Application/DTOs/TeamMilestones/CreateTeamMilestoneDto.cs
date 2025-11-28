using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class CreateTeamMilestoneDto
    {
        [Required]
        public int TeamId { get; set; }

        [Length(3, 150)]
        [Required]
        public string Title { get; set; } = string.Empty;

        [Length(0, 700)]
        public string? Description { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
    }
}
