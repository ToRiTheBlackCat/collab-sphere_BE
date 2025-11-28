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
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
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
