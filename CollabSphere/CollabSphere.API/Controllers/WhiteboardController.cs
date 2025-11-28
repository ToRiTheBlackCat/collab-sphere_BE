using CollabSphere.API.Middlewares;
using CollabSphere.Application.Features;
using CollabSphere.Application.Features.TeamWhiteboard.Commands.CreatePage;
using CollabSphere.Application.Features.TeamWhiteboard.Commands.UpdatePageTitle;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetShapeOfPage;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardByTeamId;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardPages;
using CollabSphere.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        [Authorize]
        [HttpPost("{whiteboardId}/pages")]
        public async Task<IActionResult> CreatePage(CreatePageCommand command)
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
            //Broadcast new page to connected users
            await WhiteboardSocketHandlerMiddleware.BroadcastNewPageAsync(command.WhiteboardId, JsonSerializer.Deserialize<WhiteboardPage>(result.Message) ?? new());

            return Ok(result);
        }

        [HttpGet("/api/pages/{pageId}/shapes")]
        public async Task<IActionResult> GetShapeOfPage(GetShapeOfPageQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        [HttpPut("/api/pages/{pageId}")]
        public async Task<IActionResult> UpdatePageTitle(UpdatePageTitleCommand command)
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

            //Broadcast update page to connected users
            var updatedPage = JsonSerializer.Deserialize<WhiteboardPage>(result.Message) ?? new();
            await WhiteboardSocketHandlerMiddleware.BroadcastUpdatePageAsync(updatedPage.WhiteboardId ?? 0, updatedPage);

            return Ok(result);
        }
    }
}
