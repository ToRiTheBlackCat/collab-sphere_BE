using CollabSphere.Application.Features.Classes.Commands.AddStudent;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using CollabSphere.Application.Features.Team.Commands.DeleteTeam;
using CollabSphere.Application.Features.Team.Commands.TeamUploadAvatar;
using CollabSphere.Application.Features.Team.Commands.UpdateTeam;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent;
using CollabSphere.Application.Features.Team.Queries.GetStudentTeamByAssignClass;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using CollabSphere.Application.Features.Team.Queries.GetTeamProjectOverview;
using CollabSphere.Application.Features.TeamFiles.Commands.DeleteTeamFile;
using CollabSphere.Application.Features.TeamFiles.Commands.GenerateTeamFileUrl;
using CollabSphere.Application.Features.TeamFiles.Commands.MoveTeamFile;
using CollabSphere.Application.Features.TeamFiles.Commands.UploadTeamFile;
using CollabSphere.Application.Features.TeamFiles.Queries.GetTeamFiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpGet("{teamId}/files")]
        public async Task<IActionResult> GetTeamFiles(int teamId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            int userId = int.Parse(UIdClaim.Value);
            int userRole = int.Parse(roleClaim.Value);

            var query = new GetTeamFilesQuery()
            {
                TeamId = teamId,
                UserId = userId,
                UserRole = userRole,
            };

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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpPost("{teamId}/files")]
        public async Task<IActionResult> UploadTeamFile(int teamId, [FromForm] string? pathPrefix, IFormFile file, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            int userId = int.Parse(UIdClaim.Value);
            int userRole = int.Parse(roleClaim.Value);

            var command = new UploadTeamFileCommand()
            {
                TeamId = teamId,
                FilePathPrefix = pathPrefix ?? "",
                File = file,
                UserId = userId,
                UserRole = userRole,
            };

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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpPatch("{teamId}/files/{fileId}/file-path")]
        public async Task<IActionResult> MoveTeamFile(int teamId, int fileId, [FromForm] string? pathPrefix, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            int userId = int.Parse(UIdClaim.Value);
            int userRole = int.Parse(roleClaim.Value);

            var command = new MoveTeamFileCommand()
            {
                TeamId = teamId,
                FileId = fileId,
                FilePathPrefix = pathPrefix ?? "",
                UserId = userId,
                UserRole = userRole,
            };

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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpPatch("{teamId}/files/{fileId}/new-url")]
        public async Task<IActionResult> GenerateTeamFileUrl(int teamId, int fileId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            int userId = int.Parse(UIdClaim.Value);
            int userRole = int.Parse(roleClaim.Value);

            var command = new GenerateTeamFileUrlCommand()
            {
                TeamId = teamId,
                FileId = fileId,
                UserId = userId,
                UserRole = userRole,
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(new
            {
                result.FileUrl,
                result.UrlExpireTime
            });
        }

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpDelete("{teamId}/files/{fileId}")]
        public async Task<IActionResult> DeleteTeamFile(int teamId, int fileId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            int userId = int.Parse(UIdClaim.Value);
            int userRole = int.Parse(roleClaim.Value);

            var command = new DeleteTeamFileCommand()
            {
                TeamId = teamId,
                FileId = fileId, 
                UserId = userId,
                UserRole = userRole,
            };

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

        // Roles: Student, Lecturer
        [Authorize(Roles = "4, 5")]
        [HttpGet("{teamId}/project-overview")]
        public async Task<IActionResult> GetProjectOverView(int teamId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            int userId = int.Parse(UIdClaim?.Value ?? "-1");
            int userRole = int.Parse(roleClaim?.Value ?? "-1");

            var query = new GetTeamProjectOverviewQuery()
            {
                TeamId = teamId,
                UserId = userId,
                UserRole = userRole
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result.ProjectOverview);
        }
    }
}
