using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Evaluate
{
    public class GetLecturerEvaluateTeamMilestoneDto
    {
        public int LecturerId { get; set; }
        public string? LecturerName { get; set; }
        public string? AvatarUrl { get; set; }
        public decimal? Score { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
