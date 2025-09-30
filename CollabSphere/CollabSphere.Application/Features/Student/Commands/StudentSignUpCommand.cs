using CollabSphere.Application.DTOs.Student;
using MediatR;

namespace CollabSphere.Application.Features.Student.Commands
{
    public class StudentSignUpCommand : IRequest<(bool, string)>
    {
        public StudentSignUpRequestDto Dto { get; set; }
        public StudentSignUpCommand(StudentSignUpRequestDto dto)
        {
            Dto = dto;
        }
    }
}
