using CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeam;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EvaluateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost("lecturer/team/{teamId}")]
        public async Task<IActionResult> LecturerEvaluateTeam(int teamId, [FromBody] LecturerEvaluateTeamCommand command)
        {
            if (!ModelState.IsValid)

            {
                return BadRequest(ModelState);
            }

            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.UserId = int.Parse(UIdClaim.Value);
            command.UserRole = int.Parse(roleClaim.Value);
            command.TeamId = teamId;

            var result = await _mediator.Send(command);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
    }
}
