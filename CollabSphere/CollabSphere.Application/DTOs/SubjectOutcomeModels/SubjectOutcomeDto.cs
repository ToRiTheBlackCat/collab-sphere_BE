using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectOutcomeModels
{
    public class SubjectOutcomeDto
    {
        [Required]
        public string OutcomeDetail { get; set; }
    }
}
