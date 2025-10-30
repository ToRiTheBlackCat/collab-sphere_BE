using CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Evaluate
{
    public class GetOwnEvaluationsForOtherInTeamDto
    {
        public int ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public List<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();
    }
}
