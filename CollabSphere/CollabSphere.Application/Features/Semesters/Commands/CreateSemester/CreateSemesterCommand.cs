using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Semesters.Commands.CreateSemester
{
    public class CreateSemesterCommand : ICommand, IValidatableObject
    {
        [Required]
        [MaxLength(50)]
        public string SemesterName { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string SemesterCode { get; set; } = string.Empty;
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "EndDate must be after to StartDate.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}
