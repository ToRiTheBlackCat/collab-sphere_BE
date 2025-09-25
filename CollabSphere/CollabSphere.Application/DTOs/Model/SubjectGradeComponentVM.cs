using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Model
{
    public class SubjectGradeComponentVM
    {
        public int SubjectGradeComponentId { get; set; }

        public int SubjectId { get; set; }

        public int SyllabusId { get; set; }

        public string ComponentName { get; set; }

        public decimal? ReferencePercentage { get; set; }

        public static implicit operator SubjectGradeComponentVM(SubjectGradeComponent gradeComponent)
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
    }
}
