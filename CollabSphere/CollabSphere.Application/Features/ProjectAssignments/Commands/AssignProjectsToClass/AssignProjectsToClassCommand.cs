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
        public int ClassId = -1;

        [FromBody]
        [Required]
        public HashSet<int> ProjectIds { get; set; } = new();

        public int UserId = -1;

        public int Role = -1;
    }
}
