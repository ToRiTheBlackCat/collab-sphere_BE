using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Evaluate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.LecturerGetAllEvaluationsInTeam
{
    public class LecturerGetAllEvaluationsInTeamResult : QueryResult
    {
        public List<MemberEvaluations> MemberEvaluations { get; set; } = new List<MemberEvaluations>();
    }
    public class MemberEvaluations
    {
        public int? ReceiverId { get; set; }
        public List<OtherEvaluationsForOwnInTeamDto> Evaluations { get; set; } = new List<OtherEvaluationsForOwnInTeamDto>();
    }
}
