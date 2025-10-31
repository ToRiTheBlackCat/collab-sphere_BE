using CollabSphere.Application.Features.MilestoneQues.Commands.CreateMilestoneQuestion;
using CollabSphere.Application.Features.MilestoneQues.Commands.DeleteMilestoneQuestion;
using CollabSphere.Application.Features.MilestoneQues.Commands.UpdateMilestoneQuestion;
using CollabSphere.Application.Features.MilestoneQuesAns.Commands.CreateQuestionAnswer;
using CollabSphere.Application.Features.MilestoneQuesAns.Commands.UpdateQuestionAnswer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Text.Json.Serialization;

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

        [Authorize(Roles = "4")]
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteMilestoneQuestion(int questionId)
        {
            if (!ModelState.IsValid)

            {
                return BadRequest(ModelState);
            }
            DeleteMilestoneQuestionCommand command = new();

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

        [Authorize(Roles = "5")]
        [HttpPost("{questionId}/answer")]
        public async Task<IActionResult> CreateQuestionAnswer(int questionId, [FromBody] CreateQuestionAnswerCommand command)
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

        [Authorize(Roles = "5")]
        [HttpPatch("{questionId}/answer/{answerId}")]
        public async Task<IActionResult> UpdateQuestionAnswer(int questionId, int answerId, [FromBody] UpdateQuestionAnswerCommand command)
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
