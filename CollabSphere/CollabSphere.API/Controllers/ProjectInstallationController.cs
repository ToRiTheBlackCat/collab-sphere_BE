using CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject;
using CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CollabSphere.API.Controllers
{
    [Route("api/project-installation")]
    [ApiController]
    public class ProjectInstallationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectInstallationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("/api/project/{projectId}/installation")]
        public async Task<IActionResult> GetInstallationsOfProject(GetInstallationsOfProjectQuery query, CancellationToken cancellationToken = default!)
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
        [HttpPost("/api/project/{projectId}/installation")]
        public async Task<IActionResult> CreateInstallationForProject(int projectId, [FromBody] CreateInstallationForProjectCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            command.ProjectId = projectId;
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.UserId = int.Parse(UIdClaim.Value);
            command.UserRole = int.Parse(roleClaim.Value);

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
