using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Commands.ApproveProject;
using CollabSphere.Application.Features.Project.Queries.GetAllProjects;
using CollabSphere.Application.Features.Project.Queries.GetProjectById;
using CollabSphere.Application.Features.Project.Queries.GetProjectsOfClass;
using CollabSphere.Application.Features.Project.Queries.GetTeacherProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            return Ok(result.PagedProjects);
        }

        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetTeacherProjects(GetLecturerProjectsQuery query, CancellationToken cancellationToken = default!)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.PagedProjects);
        }

        [Authorize]
        [HttpGet("{ProjectId}")]
        public async Task<IActionResult> GetProjectById(GetProjectByIdQuery query, CancellationToken cancellationToken = default!)
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

            if (result.Project == null)
            {
                return NotFound();
            }

            // Check authorization
            if (!result.IsAuthorized)
            {
                return Unauthorized(result.Message);
            }

            return Ok(result.Project);
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

        [HttpPatch("{ProjectId}/approve")]
        public async Task<IActionResult> HeadDepartmentAppoveProject(ApproveProjectCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.Message);
        }
    }
}
