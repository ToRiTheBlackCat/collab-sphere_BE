using CollabSphere.Application.Features.Checkpoints.Commands.CreateCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Queries.GetCheckpointDetail;
using CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/checkpoint")]
    [ApiController]
    public class CheckpointController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CheckpointController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpGet("{checkpointId}")]
        public async Task<IActionResult> GetCheckpointDetail(int checkpointId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct query
            var query = new GetCheckpointDetailQuery()
            {
                CheckpontId = checkpointId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle query
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            if (result.Checkpoint == null)
            {
                return NotFound();
            }

            return Ok(result.Checkpoint);
        }

        // Roles: Student
        [Authorize(Roles = "5")]
        [HttpPost]
        public async Task<IActionResult> CreateCheckpoint(CreateCheckpointCommand command, CancellationToken cancellationToken = default)
        {// Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Set claims in command
            command.UserId = int.Parse(UIdClaim.Value);
            command.UserRole = int.Parse(roleClaim.Value);

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result.ErrorList);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return CreatedAtAction(
                actionName: nameof(GetCheckpointDetail),
                routeValues: new { CheckpointId = result.CheckpointId },
                value: result
            );
        }
    }
}
