using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Features.MilestoneFiles.Commands.DeleteMilestoneFile;
using CollabSphere.Application.Features.MilestoneFiles.Commands.GenerateMilestoneFileUrl;
using CollabSphere.Application.Features.MilestoneFiles.Commands.UploadMilestoneFile;
using CollabSphere.Application.Features.MilestoneReturns.Commands.CreateMilestoneReturn;
using CollabSphere.Application.Features.MilestoneReturns.Commands.DeleteMilestoneReturn;
using CollabSphere.Application.Features.MilestoneReturns.Commands.GenerateMilestoneReturnUrl;
using CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.DeleteCustomTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.UpdateTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestonesByTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/milestone")]
    [ApiController]
    public class TeamMilestonesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamMilestonesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Roles: Lecturer, Student
        [Authorize]
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamMilestones(int teamId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct query
            var query = new GetMilestonesByTeamQuery()
            {
                TeamId = teamId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value)
            };

            // Handle command
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.TeamMilestones);
        }

        // Roles: Lecturer, Student
        [Authorize]
        [HttpGet("{teamMilestoneId}")]
        public async Task<IActionResult> GetMilestoneDetail(int teamMilestoneId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct query
            var query = new GetMilestoneDetailQuery()
            {
                TeamMilestoneId = teamMilestoneId,
                UserId = int.Parse(UIdClaim.Value),
                UserRole = int.Parse(roleClaim.Value),
            };

            // Handle command
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return result.TeamMilestone != null ? Ok(result.TeamMilestone) : NotFound();
        }

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpPost]
        public async Task<IActionResult> LecturerCreateCustomeMilestone(CreateTeamMilestoneDto milestoneDto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CreateCustomTeamMilestoneCommand()
            {
                MilestoneDto = milestoneDto,
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

            return CreatedAtAction(
                actionName: nameof(GetMilestoneDetail),
                routeValues: new { result.TeamMilestoneId },
                value: result.Message
            ); ;
        }

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpPatch("{teamMilestoneId}")]
        public async Task<IActionResult> LecturerUpdateTeamMilestone(int teamMilestoneId, UpdateTeamMilestoneDto teamMilestoneDto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            teamMilestoneDto.TeamMilestoneId = teamMilestoneId;

            // Construct command
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = teamMilestoneDto,
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

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpDelete("{teamMilestoneId}")]
        public async Task<IActionResult> LecturerDeleteTeamMilestone(int teamMilestoneId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = teamMilestoneId,
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

        // Roles: Student(Team_Leader)
        [Authorize(Roles = "5")]
        [HttpPatch("{teamMilestoneId}/status")]
        public async Task<IActionResult> TeamLeaderCheckDoneTeamMilestone(CheckTeamMilestoneDto teamMilestoneDto, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = teamMilestoneDto,
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

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpPost("{teamMilestoneId}/files")]
        public async Task<IActionResult> LecturerUploadFile(int teamMilestoneId, IFormFile formFile, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new UploadMilestoneFileCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                File = formFile,
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

        // Roles: Lecturer
        [Authorize(Roles = "4")]
        [HttpDelete("{teamMilestoneId}/files/{fileId}")]
        public async Task<IActionResult> LecturerDeleteFile(int teamMilestoneId, int fileId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new DeleteMilestoneFileCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                FileId = fileId,
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

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpPatch("{teamMilestoneId}/files/{fileId}/new-url")]
        public async Task<IActionResult> GenerateNewFileUrl(int teamMilestoneId, int fileId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new GenerateMilestoneFileUrlCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                FileId = fileId,
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

            return Ok(new
            {
                result.FileUrl,
                result.UrlExpireTime
            });
        }

        // Roles: Student
        [Authorize(Roles = "5")]
        [HttpPost("{teamMilestoneId}/returns")]
        public async Task<IActionResult> StudentSubmitReturn(int teamMilestoneId, IFormFile formFile, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new CreateMilestoneReturnCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                File = formFile,
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

        // Roles: Student
        [Authorize(Roles = "5")]
        [HttpDelete("{teamMilestoneId}/returns/{mileReturnId}")]
        public async Task<IActionResult> StudentDeleteReturn(int teamMilestoneId, int mileReturnId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new DeleteMilestoneReturnCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                MileReturnId = mileReturnId,
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

        // Roles: Lecturer, Student
        [Authorize(Roles = "4, 5")]
        [HttpPatch("{teamMilestoneId}/returns/{mileReturnId}/new-url")]
        public async Task<IActionResult> GenerateNewReturnUrl(int teamMilestoneId, int mileReturnId, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);

            // Construct command
            var command = new GenerateMilestoneReturnUrlCommand()
            {
                TeamMilestoneId = teamMilestoneId,
                MileReturnId = mileReturnId,
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

            return Ok(new
            {
                result.FileUrl,
                result.UrlExpireTime
            });
        }
    }
}
