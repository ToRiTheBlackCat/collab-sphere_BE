using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeamMilestone
{
    public class LecturerEvaluateTeamMilestoneCommand : ICommand
    {
        [JsonIgnore]
        public int TeamMilestoneId { get; set; }
        [JsonIgnore]
        public int EvaluatorId = -1;
        [JsonIgnore]
        public int EvaluatorRole = -1;
        [Required]
        [Range(0, 10)]
        public decimal Score { get; set; }
        public string? Comment { get; set; }
    }
}
