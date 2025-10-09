using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Team
{
    public class CreateTeamRequestDto
    {
        [Required]
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
