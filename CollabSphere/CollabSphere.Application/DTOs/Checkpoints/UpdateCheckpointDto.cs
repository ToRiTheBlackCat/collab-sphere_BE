using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Checkpoints
{
    public class UpdateCheckpointDto
    {
        public int CheckpointId = -1;

        [Required]
        public int TeamMilestoneId { get; set; } = -1;

        [Required]
        [StringLength(maximumLength: 150, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(maximumLength: 500, MinimumLength = 3)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 3)]
        public string Complexity { get; set; } = string.Empty;

        public DateOnly? StartDate { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }
    }
}
