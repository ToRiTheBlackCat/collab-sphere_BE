using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectGradeComponentModels
{
    public class SubjectGradeComponentVM
    {
        public int SubjectGradeComponentId { get; set; }

        public int SubjectId { get; set; }

        public int SyllabusId { get; set; }

        public string ComponentName { get; set; } = null!;

        public decimal? ReferencePercentage { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.SubjectGrades
{
    public static class SubjectGradeMappings
    {
        public static SubjectGradeComponentVM ToViewModel(this SubjectGradeComponent gradeComponent)
        {
            return new SubjectGradeComponentVM()
            {
                SubjectGradeComponentId = gradeComponent.SubjectGradeComponentId,
                ComponentName = gradeComponent.ComponentName,
                ReferencePercentage = gradeComponent.ReferencePercentage,
                SubjectId = gradeComponent.SubjectId,
                SyllabusId = gradeComponent.SyllabusId
            };
        }

        public static List<SubjectGradeComponentVM> ToViewModels(this IEnumerable<SubjectGradeComponent> gradeComponents)
        {
            return gradeComponents.Select(x => x.ToViewModel()).ToList();
        }
    }
}

