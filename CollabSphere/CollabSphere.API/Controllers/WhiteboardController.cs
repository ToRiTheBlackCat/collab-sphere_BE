using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardByTeamId;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardPages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/whiteboards")]
    [ApiController]
    public class WhiteboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WhiteboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetWhiteboardByTeamId(GetWhiteboardByTeamIdQuery query, CancellationToken cancellationToken = default)
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
        [HttpGet("{whiteboardId}/pages")]
        public async Task<IActionResult> GetWhiteboardPages(GetWhiteboardPagesQuery query, CancellationToken cancellationToken = default)
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

    }
}
