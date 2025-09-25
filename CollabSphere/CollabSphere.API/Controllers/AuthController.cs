

using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var responseDto = await _mediator.Send(new LoginCommand(dto));
            if (!responseDto.IsAuthenticated)
            {
                return NotFound(responseDto);
            }

            return Ok(responseDto);
        }
    }
}
