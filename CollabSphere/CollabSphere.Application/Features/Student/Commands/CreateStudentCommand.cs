using CollabSphere.Application.DTOs.Student;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Commands
{
    public class CreateStudentCommand : IRequest<(bool, string)>
    {
        public CreateStudentRequestDto Dto { get; set; }
        public CreateStudentCommand(CreateStudentRequestDto dto)
        {
            Dto = dto;
        }
    }
}
