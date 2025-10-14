﻿using CollabSphere.Application.Constants;
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
        [Length(3, 200)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [Length(3, 500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [MinLength(1)]
        public List<UpdateProjectObjectiveDTO> Objectives { get; set; } = new List<UpdateProjectObjectiveDTO>();

        //public Domain.Entities.Project ToProjectEntity()
        //{
        //    return new Domain.Entities.Project()
        //    {
        //        ProjectName = this.ProjectName,
        //        Description = this.Description,
        //        LecturerId = this.LecturerId,
        //        SubjectId = this.SubjectId,
        //        Status = ProjectStatuses.PENDING,
        //        Objectives = this.Objectives.Select(x => x.ToObjectiveEntity()).ToList(),
        //    };
        //}
    }
}
