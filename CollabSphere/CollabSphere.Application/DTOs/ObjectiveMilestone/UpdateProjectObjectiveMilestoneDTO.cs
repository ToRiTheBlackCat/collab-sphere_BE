using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ObjectiveMilestone
{
    public class UpdateProjectObjectiveMilestoneDTO
    {
        public int? ObjectiveMilestoneId { get; set; } = 0; // Create new if 0

        [Length(3, 150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
    }
}
