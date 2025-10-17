using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.Admin.Queries;
using CollabSphere.Application.Features.User.Commands;
using CollabSphere.Application.Features.User.Commands.SignUpHead_Staff;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
