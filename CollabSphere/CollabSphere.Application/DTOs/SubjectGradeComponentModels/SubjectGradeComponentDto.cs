using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectGradeComponentModels
{
    public class SubjectGradeComponentDto
    {
        [Required]
        public string ComponentName { get; set; }

        [Required]
        [Range(1, 100)]
        public decimal? ReferencePercentage { get; set; }
    }
}
