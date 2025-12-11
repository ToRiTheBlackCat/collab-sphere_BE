using CollabSphere.Application.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Project
{
    public class UpdateProjectDTO
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Length(3, 150)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public string BusinessRules { get; set; } = null!;

        [Required]
        public string Actors { get; set; } = null!;
    }
}
