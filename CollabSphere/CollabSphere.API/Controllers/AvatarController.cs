using CollabSphere.Application.DTOs.Image;
using CollabSphere.Application.Features.User.Commands;
using CollabSphere.Application.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/avatar")]
    [ApiController]
    public class AvatarController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AvatarController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("avatar-url")]
        public async Task<IActionResult> GetAvatarUrl(string publicId)
        {
            var imageUrl = await _mediator.Send(new GetAvatarUrlQuery(publicId));

            return Ok(imageUrl);
        }
    }
}
