using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Commands.ApproveProject;
using CollabSphere.Application.Features.Project.Commands.CreateProject;
using CollabSphere.Application.Features.Project.Commands.DeleteApprovedProject;
using CollabSphere.Application.Features.Project.Commands.DeleteProject;
using CollabSphere.Application.Features.Project.Commands.DenyProject;
using CollabSphere.Application.Features.Project.Commands.UpdateProject;
using CollabSphere.Application.Features.Project.Queries.GetAllProjects;
using CollabSphere.Application.Features.Project.Queries.GetPendingProjects;
using CollabSphere.Application.Features.Project.Queries.GetProjectById;
using CollabSphere.Application.Features.Project.Queries.GetProjectsOfClass;
using CollabSphere.Application.Features.Project.Queries.GetTeacherProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Controllers
{
    [Route("api/project")]
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

        // Roles: Lecutrer
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

        // Roles: Lecutrer, Student
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetProjectsOfClass(GetProjectsOfClassQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.PagedProjects);
        }

        // Roles: Head Department, Lecutrer, Student
        [Authorize]
        [HttpGet("{projectId}")]
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

        // Role: Head Department
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingProjects(GetPendingProjectsQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.PagedProjects);
        }

        // Role: Head Department
        [HttpPatch("{projectId}/approval")]
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

        // Roles: Lecturer
        [Authorize(Roles = "2, 3, 4")]
        [HttpPost]
        public async Task<IActionResult> TeacherCreateProject(CreateProjectCommand command, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.UserId = int.Parse(UIdClaim.Value);
            command.UserRole = int.Parse(roleClaim.Value);

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return CreatedAtAction(
                actionName: nameof(GetProjectById),
                routeValues: new { ProjectId = result.ProjectId },
                value: result
            );
        }

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpPut]
        public async Task<IActionResult> TeacherUpdateProject(UpdateProjectCommand command, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.UserId = int.Parse(UIdClaim.Value);
            command.UserRole = int.Parse(roleClaim.Value);

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Message);
        }

        [Authorize(Roles = "2, 3, 4")]
        [HttpDelete("{projectId}")]
        public async Task<IActionResult> LecturerRemoveProjectBeforeApprove(int projectId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            var command = new DeleteProjectCommand()
            {
                ProjectId = projectId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Message);
        }

        // Roles: HeadDepartment
        [Authorize(Roles = "2")]
        [HttpPatch("{projectId}/public-removal")]
        public async Task<IActionResult> HeadDepartmentRemoveProject(int projectId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new DeleteApprovedProjectCommand()
            {
                ProjectId = projectId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Message);
        }
    }
}
