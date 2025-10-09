using CollabSphere.Application.Features.Project.Commands.DenyProject;
using CollabSphere.Application.Features.Team.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeam(int teamId, [FromBody] UpdateTeamCommand command)
        {
            if (teamId != command.TeamId)
            {
                return BadRequest("Team ID in the URL does not match Team ID in the body.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result);
        }
    }
}
