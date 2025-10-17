using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.Student;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands.CreateLecturer
{
    public class CreateLecturerCommand : IRequest<(bool, string)>
    {
        public CreateLecturerRequestDto Dto { get; set; }
        public CreateLecturerCommand(CreateLecturerRequestDto dto)
        {
            Dto = dto;
        }
    }
}
