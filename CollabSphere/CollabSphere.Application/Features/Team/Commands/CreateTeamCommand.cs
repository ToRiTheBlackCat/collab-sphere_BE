using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Team;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands
{
    public class CreateTeamCommand : ICommand
    {
        [Required]
        [Length(3, 100)]
        public string TeamName { get; set; } = string.Empty;

        public string? EnrolKey { get; set; }

        public string? Description { get; set; }

        public string? GitLink { get; set; }
        [Required]
        public int LeaderId { get; set; }
        [Required]
        public int ClassId { get; set; }

        public int? ProjectAssignmentId { get; set; }
        [Required]
        public int LecturerId { get; set; }

        public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public DateOnly? EndDate { get; set; }

        public int Status { get; set; } = 1; // Default to active
    }
}
