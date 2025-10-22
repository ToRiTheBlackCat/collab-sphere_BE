using CollabSphere.Application.Features.Classes.Commands.AddStudent;
using CollabSphere.Application.Features.Team.Commands.AddStudentsToTeam;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using CollabSphere.Application.Features.Team.Commands.DeleteTeam;
using CollabSphere.Application.Features.Team.Commands.TeamUploadAvatar;
using CollabSphere.Application.Features.Team.Commands.UpdateTeam;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent;
using CollabSphere.Application.Features.Team.Queries.GetStudentTeamByAssignClass;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Controllers
{
    [Route("api/team")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamCommand command)
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

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeam(int teamId, [FromBody] UpdateTeamCommand command)
        {
            if (teamId != command.TeamId)
            {
                return BadRequest("Team ID in the URL does not match Team ID in the body.");
            }

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

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(DeleteTeamCommand command, CancellationToken cancellationToken = default!)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.UserId = int.Parse(UIdClaim.Value);
            command.UserRole = int.Parse(roleClaim.Value);

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

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

        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar(TeamUploadAvatarCommand command)
        {
            if (command.ImageFile == null || command.ImageFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (command.ImageFile.Length > 1024 * 1024)
            {
                return BadRequest("File size cannot exceed 1 MB");
            }


            var result = await _mediator.Send(command);


            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

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

        [Authorize]
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetTeamListByAssignClassOfLecturer(GetAllTeamByAssignClassQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            return Ok(result.PaginatedTeams);
        }

        [Authorize]
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetTeamListOfStudent(GetAllTeamOfStudentQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            query.ViewerUId = int.Parse(UIdClaim.Value);
            query.ViewerRole = int.Parse(roleClaim.Value);

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("student/class/{classId}")]
        public async Task<IActionResult> GetStudentTeamByAssignClas(GetStudentTeamByAssignClassQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeamDetailsById(GetTeamDetailQuery query, CancellationToken cancellationToken = default)
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
    }
}
