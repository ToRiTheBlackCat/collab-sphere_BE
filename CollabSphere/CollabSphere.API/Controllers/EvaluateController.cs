using CollabSphere.Application.Features.Evaluate.Commands.EvaluateMileQuestionAns;
using CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeam;
using CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam;
using CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluationForTeam;
using CollabSphere.Application.Features.Evaluate.Queries.GetOtherEvaluationsForOwnInTeam;
using CollabSphere.Application.Features.Evaluate.Queries.GetOwnEvaluationsForOtherInTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CollabSphere.API.Controllers
{
    [Route("api/evaluate")]
    [ApiController]
    public class EvaluateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EvaluateController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [Authorize]
        [HttpGet("lecturer/team/{teamId}")]
        public async Task<IActionResult> GetLecturerEvaluationForTeam(GetLecturerEvaluationForTeamQuery query, CancellationToken cancellationToken = default)
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
        [HttpPost("lecturer/team/{teamId}")]
        public async Task<IActionResult> LecturerEvaluateTeam(int teamId, [FromBody] LecturerEvaluateTeamCommand command)
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

        [Authorize]
        [HttpGet("member/team/{teamId}/own")]
        public async Task<IActionResult> GetOwnEvaluationsForOtherInTeam(GetOwnEvaluationsForOtherInTeamQuery query, CancellationToken cancellationToken = default)
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
        [HttpGet("member/team/{teamId}")]
        public async Task<IActionResult> GetOtherEvaluationsForOwnInTeam(GetOtherEvaluationsForOwnInTeamQuery query, CancellationToken cancellationToken = default)
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
        [HttpPost("member/team/{teamId}")]
        public async Task<IActionResult> StudentEvaluateOtherInTeam(int teamId, [FromBody] StudentEvaluateOtherInTeamCommand command)
        {
            if (!ModelState.IsValid)

            {
                return BadRequest(ModelState);
            }

            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.RaterId = int.Parse(UIdClaim.Value);
            command.RaterRole = int.Parse(roleClaim.Value);
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

        [Authorize(Roles = "5")]
        [HttpPost("answer/{answerId}")]
        public async Task<IActionResult> EvaluateMilestoneQuestionAnswer(int answerId, [FromBody] EvaluateMilestoneQuestionAnswerCommand command)
        {
            if (!ModelState.IsValid)

            {
                return BadRequest(ModelState);
            }

            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.EvaluatorId = int.Parse(UIdClaim.Value);
            command.EvaluatorRole = int.Parse(roleClaim.Value);
            command.AnswerId = answerId;

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
