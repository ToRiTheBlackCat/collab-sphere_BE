﻿using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Image;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.OTP;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.Admin.Queries;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Application.Features.OTP;
using CollabSphere.Application.Features.OTP.Commands;
using CollabSphere.Application.Features.Student;
using CollabSphere.Application.Features.Student.Commands;
using CollabSphere.Application.Features.User.Commands;
using CollabSphere.Application.Features.User.Queries;
using CollabSphere.Application.Features.User.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/user")]
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

        [HttpPost("/api/lecturer")]
        public async Task<IActionResult> CreateLecturerAccount([FromBody] CreateLecturerRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new CreateLecturerCommand(request));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }

        [HttpPost("/api/student")]
        public async Task<IActionResult> CreateStudentAccount([FromBody] CreateStudentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new CreateStudentCommand(request));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }

        [Authorize]
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(GetUserProfileByIdQuery query, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            query.ViewerUId = int.Parse(UIdClaim.Value);
            query.ViewerRole = int.Parse(roleClaim.Value);


            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            if (!result.Authorized)
            {
                return Unauthorized(result.Message);
            }

            if (result.User == null)
            {
                return NotFound();
            }

            return Ok(result);
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

        [HttpDelete("remove-avatar")]
        public async Task<IActionResult> RemoveAvatarImage([FromBody] RemoveAvatarImageDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new UserRemoveAvatarCommand(request));

            return result.Item1
              ? Ok(new { result.Item1, result.Item2 })
              : BadRequest(new { result.Item1, result.Item2 });
        }
    }
}
