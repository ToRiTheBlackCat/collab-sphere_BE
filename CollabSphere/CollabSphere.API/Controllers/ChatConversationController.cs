using CollabSphere.Application.DTOs.ChatConversations;
using CollabSphere.Application.Features.ChatConversations.Commands.CreateNewConversation;
using CollabSphere.Application.Features.ChatConversations.Commands.DeleteChatConversation;
using CollabSphere.Application.Features.ChatConversations.Commands.MarkReadUserMessages;
using CollabSphere.Application.Features.ChatConversations.Queries.GetConversationDetails;
using CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations;
using CollabSphere.Application.Features.MilestoneReturns.Commands.GenerateMilestoneReturnUrl;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Controllers
{
    [Route("api/chat-conversation")]
    [ApiController]
    public class ChatConversationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatConversationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetConversationsOfUser([FromQuery] GetUserConversationsQuery query, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            query.UserId = int.Parse(UIdClaim.Value);
            query.UserRole = int.Parse(roleClaim.Value);

            // Handle query
            var result = await _mediator.Send(query, cancellationToken);

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

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpPatch("{conversationId}/is-read")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> MarkReadMessagesInConversation(int conversationId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            var userId = int.Parse(UIdClaim.Value);
            var userRole = int.Parse(roleClaim.Value);

            // Setup Command
            var command = new MarkReadUserMessagesCommand()
            {
                ConversationId = conversationId,
                UserId = userId,
                UserRole = userRole,
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

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

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpGet("{conversationId}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetConversationDetail(int conversationId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            var userId = int.Parse(UIdClaim.Value);
            var userRole = int.Parse(roleClaim.Value);

            var query = new GetConversationDetailsQuery()
            {
                ConversationId = conversationId,
                UserId = userId,
                UserRole = userRole,
            };

            // Handle query
            var result = await _mediator.Send(query, cancellationToken);

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

        //// Roles: Lecturer, Student
        //[Authorize(Roles = "4, 5")]
        //[HttpPost]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public async Task<IActionResult> CreateNewConversation([FromBody] CreateNewChatConverastionDto dto, CancellationToken cancellationToken = default)
        //{
        //    // Get UserId & Role of requester
        //    var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        //    var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
        //    var userId = int.Parse(UIdClaim.Value);
        //    var userRole = int.Parse(roleClaim.Value);

        //    CreateNewConversationCommand command = new CreateNewConversationCommand()
        //    {
        //        ChatConversation = dto,
        //        UserId = userId,
        //        UserRole = userRole,
        //    };

        //    // Handle command
        //    var result = await _mediator.Send(command, cancellationToken);

        //    if (!result.IsValidInput)
        //    {
        //        return BadRequest(result);
        //    }

        //    if (!result.IsSuccess)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, result);
        //    }

        //    return Ok(result);
        //}

        // Roles: Lecturer, Student
        //[Authorize(Roles = "4, 5")]
        //[HttpDelete("{conversationId}")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public async Task<IActionResult> DeleteChatConversation(int conversationId, CancellationToken cancellationToken = default)
        //{
        //    // Get UserId & Role of requester
        //    var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        //    var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
        //    var userId = int.Parse(UIdClaim.Value);
        //    var userRole = int.Parse(roleClaim.Value);

        //    DeleteChatConversationCommand command = new DeleteChatConversationCommand()
        //    {
        //        ConversationId = conversationId,
        //        UserId = userId,
        //        UserRole = userRole,
        //    };

        //    // Handle command
        //    var result = await _mediator.Send(command, cancellationToken);

        //    if (!result.IsValidInput)
        //    {
        //        return BadRequest(result);
        //    }

        //    if (!result.IsSuccess)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, result);
        //    }

        //    return Ok(result);
        //}
    }
}
