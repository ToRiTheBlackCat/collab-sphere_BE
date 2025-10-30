using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.DeleteCustomTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.UpdateTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestonesByTeam;
using CollabSphere.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        // Roles: Lecturer, Student
        [Authorize]
        [HttpGet("{teamMilestoneId}")]
        public async Task<IActionResult> GetMilestoneDetail(int teamMilestoneId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct query
            var query = new GetMilestoneDetailQuery()
            {
                TeamMilestoneId = teamMilestoneId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
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

            return result.TeamMilestone != null ? Ok(result.TeamMilestone) : NotFound();
        }

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpPost]
        public async Task<IActionResult> LecturerCreateCustomeMilestone(CreateTeamMilestoneDto milestoneDto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CreateCustomTeamMilestoneCommand()
            {
                MilestoneDto = milestoneDto,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return CreatedAtAction(
                actionName: nameof(GetMilestoneDetail),
                routeValues: new { result.TeamMilestoneId },
                value: result.Message
            ); ;
        }

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpPatch("{teamMilestoneId}")]
        public async Task<IActionResult> LecturerUpdateTeamMilestone(int teamMilestoneId, UpdateTeamMilestoneDto teamMilestoneDto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            teamMilestoneDto.TeamMilestoneId = teamMilestoneId;

            // Construct command
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = teamMilestoneDto,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Message);
        }

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpDelete("{teamMilestoneId}")]
        public async Task<IActionResult> LecturerDeleteTeamMilestone(int teamMilestoneId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Message);
        }

        // Roles: Student(Team_Leader)
        [Authorize(Roles = "5")]
        [HttpPatch("{teamMilestoneId}/status")]
        public async Task<IActionResult> TeamLeaderCheckDoneTeamMilestone(CheckTeamMilestoneDto teamMilestoneDto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = teamMilestoneDto,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Message);
        }
    }
}
