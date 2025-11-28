using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Objective;
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
        [Length(0, 150)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [MinLength(1)]
        public List<UpdateProjectObjectiveDTO> Objectives { get; set; } = new List<UpdateProjectObjectiveDTO>();
    }
}
