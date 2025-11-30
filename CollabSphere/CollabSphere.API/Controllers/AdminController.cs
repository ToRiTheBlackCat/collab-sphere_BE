using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.Admin.Commands;
using CollabSphere.Application.Features.Admin.Queries;
using CollabSphere.Application.Features.Admin.Queries.AdminGetEmails;
using CollabSphere.Application.Features.Admin.Queries.GetEmailDetail;
using CollabSphere.Application.Features.User.Commands;
using CollabSphere.Application.Features.User.Commands.SignUpHead_Staff;
using Google.Apis.Gmail.v1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly EmailService emailService;

        public AdminController(IMediator mediator, EmailService emailService)
        {
            _mediator = mediator;
            this.emailService = emailService;
        }

        [HttpPost("user/head-department-staff")]
        public async Task<IActionResult> CreateHeadDepartment_StaffAccount([FromBody] HeadDepart_StaffSignUpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new HeadDepart_StaffSignUpCommand(request));

            return result.Item1
               ? Ok(new { result.Item1, result.Item2 })
               : BadRequest(new { result.Item1, result.Item2 });
        }

        [HttpGet("all-users")]
        public async Task<IActionResult> AdminGetAllUsers()
        {
            var result = await _mediator.Send(new AdminGetAllUsersQuery());

            return Ok(result);
        }

        [HttpPatch("user/{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUserAcc(DeactivateUserAccountCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);

        }

        //[Authorize(Roles = "1")]
        [HttpGet("emails")]
        public async Task<IActionResult> AdminGetEmails(AdminGetEmailsQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        [Authorize(Roles = "1")]
        [HttpGet("emails/{id}")]
        public async Task<IActionResult> GetEmailDetail(GetEmailDetailQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
    }
}
