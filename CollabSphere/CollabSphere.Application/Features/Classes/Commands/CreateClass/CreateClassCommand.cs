using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.CreateClass
{
    public class CreateClassCommand: ICommand
    {
        [Required]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int SemesterId { get; set; }

        [Required]
        public int LecturerId { get; set; }

        public string? EnrolKey { get; set; }

        [MinLength(1)]
        public List<int> StudentIds { get; set; } = new();

        [Required]
        public bool IsActive { get; set; }
    }
}
