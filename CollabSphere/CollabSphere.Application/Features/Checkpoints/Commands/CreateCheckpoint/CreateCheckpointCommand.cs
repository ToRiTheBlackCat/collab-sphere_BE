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
        [StringLength(maximumLength: 150, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(maximumLength: 500, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 3)]
        public string Complexity { get; set; }

        public DateOnly? StartDate { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }

        public int UserId = -1;

        public int UserRole = -1;
    }
}
