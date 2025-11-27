using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ObjectiveMilestone
{
    public class CreateProjectObjectiveMilestoneDTO
    {
        [Length(3, 100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public Domain.Entities.ObjectiveMilestone ToObjectiveMilestoneEntity()
        {
            return new Domain.Entities.ObjectiveMilestone()
            {
                Title = this.Title,
                Description = this.Description,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
            };
        }
    }
}
