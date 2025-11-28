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
        [Required]
        public int CheckpointId = -1;

        [Required]
        public int TeamMilestoneId { get; set; } = -1;

        [Required]
        [Length(3, 150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Length(3, 500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Length(3, 50)]
        public string Complexity { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }
    }
}
