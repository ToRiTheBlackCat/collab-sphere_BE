using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CollabSphere.Application.Features.Team.Commands
{
    public class UpdateTeamCommand : ICommand
    {
        [Required]
        public int TeamId { get; set; }
        [Required]
        [Length(3, 100)]
        public string TeamName { get; set; } = string.Empty;

        public string? EnrolKey { get; set; }

        public string? Description { get; set; }

        public string? GitLink { get; set; }
        [Required]
        public int ClassId { get; set; }

        public int? ProjectAssignmentId { get; set; }

        public DateOnly? EndDate { get; set; }

        public int Status { get; set; }
    }
}
