using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Image;
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
using CollabSphere.Application.Features.User.Queries;
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

        [HttpPost("upload-avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequestDTO request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (request.File.Length > 1024 * 1024)
            {
                return BadRequest("File size cannot exceed 1 MB");
            }

            var result = await _mediator.Send(new UserUploadAvatarCommand(request.File, request.UserId, "avatars"));

            return result.Item1
              ? Ok(new { result.Item1, result.Item2 })
              : BadRequest(new { result.Item1, result.Item2 });
        }

        [HttpGet("avatar-url")]
        public async Task<IActionResult> GetAvatarUrl(string publicId)
        {
            var imageUrl = await _mediator.Send(new GetAvatarUrlQuery(publicId));

            return Ok(imageUrl);
        }
    }
}
