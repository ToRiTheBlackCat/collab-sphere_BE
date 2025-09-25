using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Staff.Commands.CreateClass
{
    public class CreateClassCommand: ICommand<BaseCommandResult>
    {
        [Required]
        public string ClassName { get; init; } = string.Empty;

        [Required]
        public int SubjectId { get; init; }

        [Required]
        public int LecturerId { get; init; }

        [Required]
        public string EnrolKey { get; init; }

        [MinLength(1)]
        public List<int> StudentIdList { get; init; } = new();

        [Required]
        public bool IsActive { get; init; }
    }
}
