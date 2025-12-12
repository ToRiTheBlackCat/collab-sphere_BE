using CollabSphere.Application.Features.TeamMemberEvaluation.Commands.CreateTeamMemberEvaluationsForTeam;
using CollabSphere.Application.Features.TeamMemberEvaluation.Queries.GetTeamMemberEvaluationsForTeam;
using CollabSphere.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/team-mem-evaluate")]
    [ApiController]
    public class TeamMemEvaluationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamMemEvaluationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamMemberEvaluationsForTeam(GetTeamMemberEvaluationsForTeamQuery query, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            query.UserId = int.Parse(UIdClaim.Value);
            query.UserRole = int.Parse(roleClaim.Value);

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("team/{teamId}")]
        public async Task<IActionResult> CreateTeamMemberEvaluationsForTeam(int teamId, [FromBody] CreateTeamMemberEvaluationsForTeamCommand command, CancellationToken cancellationToken = default)
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
