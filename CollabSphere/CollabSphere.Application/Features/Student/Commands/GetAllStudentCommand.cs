using CollabSphere.Application.DTOs.Student;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Commands
{
    public class GetAllStudentCommand : IRequest<GetAllStudentRequestDto>
    {
        public GetAllStudentRequestDto Dto { get; set; }
        public GetAllStudentCommand(GetAllStudentRequestDto dto)
        {
            Dto = dto;
        }
    }
}
