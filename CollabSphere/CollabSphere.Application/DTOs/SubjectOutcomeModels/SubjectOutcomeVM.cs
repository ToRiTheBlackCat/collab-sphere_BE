using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
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

        public string OutcomeDetail { get; set; } = null!;
    }
}

namespace CollabSphere.Application.Mappings.SubjectOutcomes
{
    public static partial class SubjectOutcomeMappings
    {
        public static SubjectOutcomeVM ToViewModel(this SubjectOutcome outcome)
        {
            return new SubjectOutcomeVM()
            {
                SubjectOutcomeId = outcome.SubjectOutcomeId,
                OutcomeDetail = outcome.OutcomeDetail,
                SyllabusId = outcome.SyllabusId
            };
        }

        public static List<SubjectOutcomeVM> ToViewModels(this IEnumerable<SubjectOutcome> outcomes)
        {
            return outcomes.Select(x => x.ToViewModel()).ToList();
        }
    }
}
