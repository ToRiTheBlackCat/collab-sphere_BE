using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectModels
{
    public class ImportSubjectDto
    {
        [Required]
        public string SubjectCode { get; set; }

        [Required]
        [MinLength(4)]
        public string SubjectName { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public ImportSubjectSyllabusDto SubjectSyllabus { get; set; }
    }
}
