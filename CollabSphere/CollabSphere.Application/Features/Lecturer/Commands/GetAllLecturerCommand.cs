using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Lecturer;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands
{
    public class GetAllLecturerCommand : IRequest<GetAllLecturerResponseDto>
    {
        public GetAllLecturerRequestDto Dto { get; set; }
        public GetAllLecturerCommand(GetAllLecturerRequestDto dto)
        {
            Dto = dto;
        }
    }
}
