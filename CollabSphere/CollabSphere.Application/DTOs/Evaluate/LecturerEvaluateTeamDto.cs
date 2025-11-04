using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Evaluate
{
    public class LecturerEvaluateTeamDto
    {
        public decimal? FinalGrade { get; set; }
        public string? TeamComment { get; set; }
        public List<EvaluateDetailForTeam> EvaluateDetails { get; set; } = new List<EvaluateDetailForTeam>();
    }

    public class EvaluateDetailForTeam
    {
        public int? SubjectGradeComponentId { get; set; }
        public string? SubjectGradeComponentName { get; set; }
        public decimal Score { get; set; }
        public string? DetailComment { get; set; }
    }
}
