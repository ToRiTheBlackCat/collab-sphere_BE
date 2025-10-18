using CollabSphere.Application.Features.TeamMilestones.GetMilestonesByTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CollabSphere.API.Controllers
{
    [Route("api/milestone")]
    [ApiController]
    public class TeamMilestonesController : ControllerBase
    {

        private readonly IMediator _mediator;

        public TeamMilestonesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Roles: Lecturer, Student
        [Authorize]
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamMilestones(int teamId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct query
            var query = new GetMilestonesByTeamQuery()
            {
                TeamId = teamId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value)
            };

            // Handle command
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.TeamMilestones);
        }
    }
}
