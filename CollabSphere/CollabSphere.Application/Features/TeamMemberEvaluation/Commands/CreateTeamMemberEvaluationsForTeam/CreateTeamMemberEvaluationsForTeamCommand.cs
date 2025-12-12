using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamMemEvaluation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMemberEvaluation.Commands.CreateTeamMemberEvaluationsForTeam
{
    public class CreateTeamMemberEvaluationsForTeamCommand : ICommand
    {
        [JsonIgnore]
        public int TeamId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;
        [Required]
        public List<MemberEvaluationDto> MemberScores { get; set; } = new List<MemberEvaluationDto>();
    }
}
