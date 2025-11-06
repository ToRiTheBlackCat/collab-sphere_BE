using CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/projectrepository")]
    [ApiController]
    public class ProjectRepoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectRepoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/api/projects/{projectId}/repositories")]
        public async Task<IActionResult> GetReposOfProject(GetReposOfProjectQuery query, CancellationToken cancellationToken = default!)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.PaginatedRepos);
        }

        [HttpPost("/api/internal/project-repositories")]
        public async Task<IActionResult> CreateRepoForProject([FromBody] CreateTeamCommand command)
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
