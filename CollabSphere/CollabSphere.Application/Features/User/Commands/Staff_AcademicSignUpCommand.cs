using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Commands
{
    public class Staff_AcademicSignUpCommand : IRequest<(bool, string)>
    {
        public Staff_AcademicSignUpRequestDto Dto { get; set; }
        public Staff_AcademicSignUpCommand(Staff_AcademicSignUpRequestDto dto)
        {
            Dto = dto;
        }
    }
}
