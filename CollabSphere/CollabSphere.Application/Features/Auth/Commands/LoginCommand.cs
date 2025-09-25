using CollabSphere.Application.DTOs.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<LoginResponseDTO>
    {
        public LoginRequestDTO Dto { get; set; }
        public LoginCommand(LoginRequestDTO dto)
        {
            Dto = dto;
        }
    }
}
