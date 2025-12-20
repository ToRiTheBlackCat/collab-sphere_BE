using CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Evaluate
{
    public class OtherEvaluationsForOwnInTeamDto
    {
        public int RaterId { get; set; }
        public string? RaterName { get; set; }
        public string? RaterAvatar { get; set; }
        public string? RaterCode { get; set; }
        public int? RaterTeamRole { get; set; }

        public int? ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverCode { get; set; }

        public List<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();
    }
}
