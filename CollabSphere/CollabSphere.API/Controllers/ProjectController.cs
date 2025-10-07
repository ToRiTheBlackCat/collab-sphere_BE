using CollabSphere.Application.Features.Project.Queries.GetAllProjects;
using CollabSphere.Application.Features.Project.Queries.GetProjectsOfClass;
using CollabSphere.Application.Features.Project.Queries.GetTeacherProjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Roles - Head, Lecturer
        [HttpGet]
        public async Task<IActionResult> GetAllProjects(GetAllProjectsQuery query)
        {
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.Projects);
        }

        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetTeacherProjects(GetLecturerProjectsQuery query, CancellationToken cancellationToken = default!)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.Projects);
        }

        [HttpGet("class/{ClassId}")]
        public async Task<IActionResult> GetTeacherProjects(GetProjectsOfClassQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.PagedProjects);
        }
    }
}
