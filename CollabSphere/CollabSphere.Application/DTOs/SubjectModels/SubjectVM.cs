using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using CollabSphere.Application.Mappings.SubjectSyllabuses;
using CollabSphere.Domain.Entities;

namespace CollabSphere.Application.DTOs.SubjectModels
{
    public class SubjectVM
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = null!;

        public string SubjectCode { get; set; } = null!;

        public bool IsActive { get; set; }

        public SubjectSyllabusVM? SubjectSyllabus { get; set; }

        public static explicit operator SubjectVM(Subject subject)
        {
            if (subject == null)
            {
                throw new ArgumentNullException();
            }

            var syllabusVm = subject.SubjectSyllabi.FirstOrDefault()?.ToViewModel();

            return new SubjectVM()
            {
                SubjectId = subject.SubjectId,
                SubjectName = subject.SubjectName,
                SubjectCode = subject.SubjectCode,
                IsActive = subject.IsActive,
                SubjectSyllabus = syllabusVm,
            };
        }
    }
}
