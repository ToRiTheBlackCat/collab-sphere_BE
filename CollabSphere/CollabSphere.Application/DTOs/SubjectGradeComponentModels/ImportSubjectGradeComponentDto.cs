using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectGradeComponentModels
{
    public class ImportSubjectGradeComponentDto
    {
        [Required]
        [MinLength(4)]
        public string ComponentName { get; set; }

        [Range(0.0,100.0)]
        public decimal? ReferencePercentage { get; set; }
    }
}
