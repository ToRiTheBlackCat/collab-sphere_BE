using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Project
{
    public class CreateProjectDTO
    {
        [Required]
        [Length(3, 200)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [Length(3, 500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int LecturerId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateProjectObjectiveDTO> Objectives { get; set; } = new List<CreateProjectObjectiveDTO>();

        public Domain.Entities.Project ToProjectEntity()
        {
            return new Domain.Entities.Project()
            {
                ProjectName = this.ProjectName,
                Description = this.Description,
                LecturerId = this.LecturerId,
                SubjectId = this.SubjectId,
                Status = (int)ProjectStatuses.PENDING,
                Objectives = this.Objectives.Select(x => x.ToObjectiveEntity()).ToList(),
            };
        }
    }
}
