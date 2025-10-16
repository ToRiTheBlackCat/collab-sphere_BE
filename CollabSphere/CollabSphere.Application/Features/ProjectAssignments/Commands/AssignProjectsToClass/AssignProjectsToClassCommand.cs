using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectAssignments.Commands.AssignProjectsToClass
{
    public class AssignProjectsToClassCommand : ICommand
    {
        [FromRoute(Name = "classId")]
        public int ClassId { get; set; }

        [FromBody]
        [Required]
        [MinLength(1)]
        public HashSet<int> ProjectIds { get; set; } = new();

        public int UserId = -1;

        public int Role = -1;
    }
}
