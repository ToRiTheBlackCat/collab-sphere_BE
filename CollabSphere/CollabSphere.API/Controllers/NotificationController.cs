using CollabSphere.Application.Common;
using CollabSphere.Application.Features.Notifications.Commands.MarkReadNotification;
using CollabSphere.Application.Features.Notifications.Queries.GetNotifcationsOfUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpGet]
        public async Task<IActionResult> GetNotificationsOfUser(GetNotifcationsOfUserQuery query, CancellationToken cancellationToken = default)
        {
            // Get information of requester
            var userInfo = this.GetCurrentUserInfo();

            query.UserId = userInfo.UserId;
            query.UserRole = userInfo.RoleId;

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }
            else if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpPatch("{notificationId}/is-read")]
        public async Task<IActionResult> GetNotificationsOfUser(int notificationId, CancellationToken cancellationToken = default)
        {
            // Get information of requester
            var userInfo = this.GetCurrentUserInfo();

            var command = new MarkReadNotificationCommand()
            {
                NotificationId = notificationId,
                UserId = userInfo.UserId,
                UserRole = userInfo.RoleId,
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }
            else if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
    }
}
