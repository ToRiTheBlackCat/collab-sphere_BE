using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectSyllabusModel
{
    public class ImportSubjectSyllabusDto
    {
        [Required]
        [MinLength(4)]
        public string SyllabusName { get; set; }

        [Required]
        [MinLength(12)]
        public string Description { get; set; }

        [Required]
        public string SubjectCode { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? NoCredit { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public List<ImportSubjectGradeComponentDto> SubjectGradeComponents { get; set; } = new List<ImportSubjectGradeComponentDto>();

        public List<ImportSubjectOutcomeDto> SubjectOutcomes { get; set; } = new List<ImportSubjectOutcomeDto>();
    }
}
