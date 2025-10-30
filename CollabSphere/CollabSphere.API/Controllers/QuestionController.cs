using CollabSphere.Application.Features.MilestoneQues.Commands.CreateMilestoneQuestion;
using CollabSphere.Application.Features.MilestoneQues.Commands.UpdateMilestoneQuestion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "4")]
        [HttpPost("milestone/{teamMilestoneId}")]
        public async Task<IActionResult> CreateMilestoneQuestion(int teamMilestoneId, [FromBody] CreateMilestoneQuestionCommand command)
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
            command.TeamMilestoneId = teamMilestoneId;

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

        [Authorize(Roles = "4")]
        [HttpPatch("{questionId}")]
        public async Task<IActionResult> UpdateMilestoneQuestion(int questionId, [FromBody] UpdateMilestoneQuestionCommand command)
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
            command.QuestionId = questionId;

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
