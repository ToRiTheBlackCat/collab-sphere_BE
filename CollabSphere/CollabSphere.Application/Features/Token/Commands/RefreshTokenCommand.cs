using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Token;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Token.Commands
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResponseDto>
    {
        public RefreshTokenRequestDto Dto { get; set; }
        public RefreshTokenCommand(RefreshTokenRequestDto dto)
        {
            Dto = dto;
        }
    }
}
