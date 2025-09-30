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
    public class HeadDepart_StaffSignUpCommand : IRequest<(bool, string)>
    {
        public HeadDepart_StaffSignUpRequestDto Dto { get; set; }
        public HeadDepart_StaffSignUpCommand(HeadDepart_StaffSignUpRequestDto dto)
        {
            Dto = dto;
        }
    }
}
