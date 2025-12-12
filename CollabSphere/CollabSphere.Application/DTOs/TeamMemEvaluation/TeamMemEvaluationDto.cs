using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMemEvaluation
{
    public class TeamMemEvaluationDto
    {
        public int TeamId { get; set; }
        public int LecturerId { get; set; }
        public List<MemberEvaluationDto> MemberScores { get; set; } = new List<MemberEvaluationDto>();
    }
    public class MemberEvaluationDto
    {
        public int ClassMemberId { get; set; }
        public string? MemberName { get; set; }
        public decimal? Score { get; set; }
    }
}
