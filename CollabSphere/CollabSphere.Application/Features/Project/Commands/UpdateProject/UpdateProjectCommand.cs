using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using System.ComponentModel.DataAnnotations;

namespace CollabSphere.Application.Features.Project.Commands.UpdateProject
{
    public class UpdateProjectCommand : ICommand
    {
        [Required]
        public UpdateProjectDTO Project { get; set; } = default!;

        public int UserId = -1; // Lecturer ID

        public int UserRole = -1; // Expected: Lecturer Role
    }
}
