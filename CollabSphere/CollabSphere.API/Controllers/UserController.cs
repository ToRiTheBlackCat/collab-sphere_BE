using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.OTP;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Application.Features.OTP;
using CollabSphere.Application.Features.OTP.Commands;
using CollabSphere.Application.Features.Student;
using CollabSphere.Application.Features.Student.Commands;
using CollabSphere.Application.Features.User.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("signup/send-otp")]
        public async Task<IActionResult> SendOTPToMail([FromBody] OTPRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new SendOTPCommand(request.Email));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }

        [HttpPost("student/confirm-signup")]
        public async Task<IActionResult> ConfirmSignUpStudentAccount([FromBody] StudentSignUpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new StudentSignUpCommand(request));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }

        [HttpPost("lecturer/signup")]
        public async Task<IActionResult> SignUpLecturerAccount([FromBody] LecturerSignUpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new LecturerSignUpCommand(request));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }
        [HttpPost("staff_academic/signup")]
        public async Task<IActionResult> SignUpStaff_AcademicAccount([FromBody] Staff_AcademicSignUpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new Staff_AcademicSignUpCommand(request));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }
    }
}
