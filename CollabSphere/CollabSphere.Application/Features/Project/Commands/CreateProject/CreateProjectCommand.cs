using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.CreateProject
{
    public class CreateProjectCommand : ICommand<CreateProjectResult>
    {
        [Required]
        public CreateProjectDTO Project { get; set; } = default!;

        public int UserId = -1; // Lecturer ID

        public int UserRole = -1; // Expected: Lecturer Role
    }
}
