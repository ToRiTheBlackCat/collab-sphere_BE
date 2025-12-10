using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using CollabSphere.Application.DTOs.SyllabusMilestones;
using CollabSphere.Application.Mappings.SubjectGrades;
using CollabSphere.Application.Mappings.SubjectOutcomes;
using CollabSphere.Application.Mappings.SyllabusMilestones;
using CollabSphere.Domain.Entities;

namespace CollabSphere.Application.DTOs.SubjectSyllabusModel
{
    public class SubjectSyllabusVM
    {
        public int SyllabusId { get; set; }

        public string SyllabusName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string SubjectCode { get; set; } = null!;

        public int SubjectId { get; set; }

        public int? NoCredit { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        public List<SubjectGradeComponentVM> SubjectGradeComponents { get; set; } = new List<SubjectGradeComponentVM>();

        public List<SubjectOutcomeVM> SubjectOutcomes { get; set; } = new List<SubjectOutcomeVM>();

        public List<SyllabusMilestoneVM> SyllabusMilestones { get; set; } = new List<SyllabusMilestoneVM>();
    }
}

namespace CollabSphere.Application.Mappings.SubjectSyllabuses
{
    public static class SubjectSyllabusMappings
    {
        public static SubjectSyllabusVM ToViewModel(this SubjectSyllabus subjectSyllabus)
        {
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
                SubjectGradeComponents = subjectSyllabus.SubjectGradeComponents.ToViewModels(),
                SubjectOutcomes = subjectSyllabus.SubjectOutcomes.ToViewModels(),
                SyllabusMilestones = subjectSyllabus.SyllabusMilestones.ToViewModel(),
            };
        }

        public static List<SubjectSyllabusVM> ToViewModels(this IEnumerable<SubjectSyllabus> subjectSyllabuses)
        {
            if (subjectSyllabuses == null || !subjectSyllabuses.Any())
            {
                return new List<SubjectSyllabusVM>();
            }

            return subjectSyllabuses.Select(x => x.ToViewModel()).ToList();
        }
    }
}
