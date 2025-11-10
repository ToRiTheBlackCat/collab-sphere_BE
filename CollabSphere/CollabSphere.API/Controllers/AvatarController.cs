using Amazon.Runtime.Internal;
using CollabSphere.Application.DTOs.Image;
using CollabSphere.Application.Features.User.Commands;
using CollabSphere.Application.Features.User.Queries;
using CollabSphere.Application.Features.VideoRecord.Commands.UploadRecord;
using Google.Apis.Drive.v3;
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

        [HttpPost("/api/video/uploadation")]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> UploadVideoRecord(UploadRecordCommand command)
        {
           
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
