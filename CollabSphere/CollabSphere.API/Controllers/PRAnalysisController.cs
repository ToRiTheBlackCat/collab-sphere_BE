using CollabSphere.Application.Features.PrAnalysis.Commands.UpsertPrAnalysis;
using CollabSphere.Application.Features.PrAnalysis.Queries.GetDetailOfAnalysis;
using CollabSphere.Application.Features.PrAnalysis.Queries.GetListOfAnalysis;
using CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject;
using CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/pr-analysis")]
    [ApiController]
    public class PRAnalysisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PRAnalysisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpsertPrAnalysis([FromBody] UpsertPrAnalysisCommand command)
        {
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

        [Authorize]
        [HttpGet("{analysisId}")]
        public async Task<IActionResult> GetDetailOfAnalysis(GetDetailOfAnalysisQuery query, CancellationToken cancellationToken = default!)
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
        [HttpGet("/api/team/{teamId}/pr-analysis")]
        public async Task<IActionResult> GetListAnalysisOfTeam(GetListAnalysisOfTeamQuery query, CancellationToken cancellationToken = default!)
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
