using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectModels
{
    public class CreateSubjectDto
    {
        [Required]
        [Length(3, 150)]
        public string SubjectName { get; set; } = null!;

        [Required]
        [Length(3, 40)]
        public string SubjectCode { get; set; } = null!;

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public SubjectSyllabusDto SubjectSyllabus { get; set; } = null!;
    }

    public class SubjectSyllabusDto
    {
        [Required]
        [Length(3, 200)]
        public string SyllabusName { get; set; } = null!;

        [Required]
        [Length(3, 500)]
        public string Description { get; set; } = null!;

        [Required]
        public int NoCredit { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [MinLength(1)]
        public List<SubjectGradeComponentDto> SubjectGradeComponents { get; set; } = new();

        [MinLength(1)]
        public List<SubjectOutcomeDto> SubjectOutcomes { get; set; } = new();
    }

    public class SubjectGradeComponentDto
    {
        [Required]
        [Length(1, 80)]
        public string ComponentName { get; set; } = null!;

        [Required]
        [Range(0, 100)]
        public decimal ReferencePercentage { get; set; }
    }

    public class SubjectOutcomeDto
    {
        [Required]
        [Length(1, 500)]
        public string OutcomeDetail { get; set; } = null!;

        [MinLength(1)]
        public List<SyllabusMilestoneDto> SyllabusMilestones { get; set; } = new();
    }

    public class SyllabusMilestoneDto
    {
        [Required]
        [Length(1, 200)]
        public string Title { get; set; } = null!;

        [Required]
        [Length(1, 500)]
        public string Description { get; set; } = null!;

        public DateOnly StarDate { get; set; }

        public DateOnly EndDate { get; set; }
    }
}
