using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using System.ComponentModel.DataAnnotations;

namespace CollabSphere.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommand : ICommand
    {
        [Required]
        public string SubjectName { get; set; }

        [Required]
        public string SubjectCode { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public SubjectSyllabusDto SubjectSyllabus { get; set; }
    }
}
