using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectOutcomeModels
{
    public class SubjectOutcomeVM
    {
        public int SubjectOutcomeId { get; set; }

        public int SyllabusId { get; set; }

        public string OutcomeDetail { get; set; }

        public static implicit operator SubjectOutcomeVM(SubjectOutcome outcome)
        {
            return new SubjectOutcomeVM()
            {
                SubjectOutcomeId = outcome.SubjectOutcomeId,
                OutcomeDetail = outcome.OutcomeDetail,
                SyllabusId = outcome.SyllabusId
            };
        }
    }
}
