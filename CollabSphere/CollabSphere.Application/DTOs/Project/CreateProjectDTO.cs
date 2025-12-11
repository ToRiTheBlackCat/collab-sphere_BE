using CollabSphere.Application.Constants;
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
        public string ProjectName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int LecturerId { get; set; }

        public int SubjectId { get; set; }

        public string BusinessRules { get; set; } = null!;

        public string Actors { get; set; } = null!;

        public Domain.Entities.Project ToProjectEntity()
        {
            return new Domain.Entities.Project()
            {
                ProjectName = this.ProjectName,
                Description = this.Description,
                LecturerId = this.LecturerId,
                SubjectId = this.SubjectId,
                Status = (int)ProjectStatuses.PENDING,
                BusinessRules = this.BusinessRules,
                Actors = this.Actors,
            };
        }
    }
}
