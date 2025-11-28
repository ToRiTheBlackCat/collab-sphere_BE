using CollabSphere.Application.DTOs.ObjectiveMilestone;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Objective
{
    public class UpdateProjectObjectiveDTO
    {
        public int ObjectiveId { get; set; } // Create new if 0

        public string? Description { get; set; }

        [Required]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<UpdateProjectObjectiveMilestoneDTO> ObjectiveMilestones { get; set; } = new List<UpdateProjectObjectiveMilestoneDTO>();
    }
}
