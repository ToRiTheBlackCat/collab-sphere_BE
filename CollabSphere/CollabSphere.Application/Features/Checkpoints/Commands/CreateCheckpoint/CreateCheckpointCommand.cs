using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CreateCheckpoint
{
    public class CreateCheckpointCommand : ICommand<CreateCheckpointResult>
    {
        [Required]
        public int TeamMilestoneId { get; set; } = -1;

        [Required]
        [Length(3, 150)]
        public string Title { get; set; } = null!;

        [Required]
        [Length(3, 500)]
        public string Description { get; set; } = null!;

        [Required]
        [Length(3, 50)]
        public string Complexity { get; set; } = null!;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }

        public int UserId = -1;

        public int UserRole = -1;
    }
}
