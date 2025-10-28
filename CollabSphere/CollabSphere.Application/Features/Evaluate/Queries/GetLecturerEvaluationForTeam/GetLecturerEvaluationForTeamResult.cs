using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Evaluate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluationForTeam
{
    public class GetLecturerEvaluationForTeamResult : QueryResult
    {
        public LecturerEvaluateTeamDto? LecturerEvaluateTeam { get; set; }
    }
}
