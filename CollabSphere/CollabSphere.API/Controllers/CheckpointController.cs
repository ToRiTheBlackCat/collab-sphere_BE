using CollabSphere.Application.DTOs.CheckpointAssignments;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.Features.Checkpoints.Commands.AssignMembersToCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Commands.CheckDoneCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Commands.CheckpointDeleteFile;
using CollabSphere.Application.Features.Checkpoints.Commands.CheckpointUploadFile;
using CollabSphere.Application.Features.Checkpoints.Commands.CreateCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Commands.DeleteCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Commands.GenerateCheckpointFileUrl;
using CollabSphere.Application.Features.Checkpoints.Commands.UpdateCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Queries.GetCheckpointDetail;
using CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone;
using CollabSphere.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpPost]
        public async Task<IActionResult> CreateCheckpoint(CreateCheckpointCommand command, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpPut("{checkpointId}")]
        public async Task<IActionResult> UpdateCheckpoint(int checkpointId, UpdateCheckpointDto dto, CancellationToken cancellationToken = default)
        {
            dto.CheckpointId = checkpointId;

            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Set claims in command
            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = dto,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

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

            return Ok(result.Message);
        }

        // Roles: Student
        [Authorize(Roles = "5")]
        [HttpPatch("{checkpointId}/status")]
        public async Task<IActionResult> CheckDoneCheckpoint(CheckDoneCheckpointCommand command, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

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

            return Ok(result.Message);
        }

        // Roles: Student
        [Authorize(Roles = "5")]
        [HttpPost("{checkpointId}/assignments")]
        public async Task<IActionResult> AssignMembersToCheckpoint(CheckpointAssignmentsDto dto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new AssignMembersToCheckpointCommand()
            {
                AssignmentsDto = dto,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

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

            return Ok(result.Message);
        }

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpDelete("{checkpointId}")]
        public async Task<IActionResult> DeleteCheckpoint(int checkpointId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new DeleteCheckpointCommand()
            {
                CheckpointId = checkpointId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

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

            return Ok(result.Message);
        }

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpPost("{checkpointId}/files")]
        public async Task<IActionResult> UploadFile(int checkpointId, IFormFile checkpointFile, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CheckpointUploadCommand()
            {
                CheckpointId = checkpointId,
                File = checkpointFile,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

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

            return Ok(result.Message);
        }

        [Authorize(Roles = "4, 5")]
        [HttpDelete("{checkpointId}/files/{fileId}")]
        public async Task<IActionResult> RemoveCheckpointFile(int checkpointId, int fileId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CheckpointDeleteFileCommand()
            {
                CheckpointId = checkpointId,
                FileId = fileId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

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

            return Ok(result.Message);
        }
    }
}
