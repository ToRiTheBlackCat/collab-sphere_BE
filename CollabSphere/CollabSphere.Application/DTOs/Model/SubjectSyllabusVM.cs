using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Model
{
    public class SubjectSyllabusVM
    {
        public int? SyllabusId { get; set; }

        public string? SyllabusName { get; set; }

        public string? Description { get; set; }

        public string? SubjectCode { get; set; }

        public int? SubjectId { get; set; }

        public int? NoCredit { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? IsActive { get; set; }

        public List<SubjectGradeComponentVM>? SubjectGradeComponents { get; set; }

        public List<SubjectOutcomeVM>? SubjectOutcomes { get; set; }

        public static implicit operator SubjectSyllabusVM?(SubjectSyllabus subjectSyllabus)
        {
            if (subjectSyllabus == null)
            {
                return null;
            }

            return new SubjectSyllabusVM()
            {
                SyllabusId = subjectSyllabus.SyllabusId,
                Description = subjectSyllabus.Description,
                CreatedDate = subjectSyllabus.CreatedDate,
                IsActive = subjectSyllabus.IsActive,
                NoCredit = subjectSyllabus.NoCredit,
                SubjectCode = subjectSyllabus.SubjectCode,
                SubjectId = subjectSyllabus.SubjectId,
                SyllabusName = subjectSyllabus.SyllabusName,
                SubjectGradeComponents = subjectSyllabus.SubjectGradeComponents
                    .Select(x => (SubjectGradeComponentVM)x).ToList(),
                SubjectOutcomes = subjectSyllabus.SubjectOutcomes
                    .Select(x => (SubjectOutcomeVM)x).ToList(),
            };
        }
    }
}
