using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollabSphere.Application.Features.Subjects.UpdateSubject
{
    public class UpdateSubjectCommand : ICommand<BaseCommandResult>
    {
        [Required]
        public int SubjectId { get; set; }

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
