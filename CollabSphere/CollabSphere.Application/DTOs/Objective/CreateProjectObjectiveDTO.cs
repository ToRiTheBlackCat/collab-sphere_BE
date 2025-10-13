using CollabSphere.Application.DTOs.ObjectiveMilestone;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Objective
{
    public class CreateProjectObjectiveDTO
    {
        //public int ObjectiveId { get; set; } // No Need: Create identity and Never exit before Project

        [Required]
        [Length(3, 450)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Length(3, 40)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<CreateProjectObjectiveMilestoneDTO> ObjectiveMilestones { get; set; } = new List<CreateProjectObjectiveMilestoneDTO>();

        public Domain.Entities.Objective ToObjectiveEntity()
        {
            return new Domain.Entities.Objective()
            {
                Description = this.Description,
                Priority = this.Priority,
                ObjectiveMilestones = this.ObjectiveMilestones.Select(x => x.ToObjectiveMilestoneEntity()).ToList(),
            };
        }
    }
}
