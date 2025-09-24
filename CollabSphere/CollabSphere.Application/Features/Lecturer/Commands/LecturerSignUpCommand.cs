using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.Student;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands
{
    public class LecturerSignUpCommand : IRequest<(bool, string)>
    {
        public LecturerSignUpRequestDto Dto { get; set; }
        public LecturerSignUpCommand(LecturerSignUpRequestDto dto)
        {
            Dto = dto;
        }
    }
}
