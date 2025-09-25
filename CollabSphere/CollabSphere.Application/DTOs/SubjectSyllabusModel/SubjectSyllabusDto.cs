using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectSyllabusModel
{
    public class SubjectSyllabusDto
    {
        [Required]
        public string SyllabusName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int? NoCredit { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [MinLength(1)]
        public List<SubjectGradeComponentDto> SubjectGradeComponents { get; set; } = new();

        [MinLength(1)]
        public List<SubjectOutcomeDto> SubjectOutcomes { get; set; } = new();
    }
}
