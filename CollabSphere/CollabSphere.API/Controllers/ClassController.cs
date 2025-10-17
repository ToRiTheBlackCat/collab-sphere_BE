using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.Features.Classes.Commands.AssignLec;
using CollabSphere.Application.Features.Classes.Commands.AddStudent;
using CollabSphere.Application.Features.Classes.Commands.CreateClass;
using CollabSphere.Application.Features.Classes.Commands.ImportClass;
using CollabSphere.Application.Features.Classes.Queries.GetAllClasses;
using CollabSphere.Application.Features.Classes.Queries.GetClassById;
using CollabSphere.Application.Features.Classes.Queries.GetLecturerClasses;
using CollabSphere.Application.Features.Classes.Queries.GetStudentClasses;
using CollabSphere.Application.Features.ProjectAssignments.Commands.AssignProjectsToClass;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CollabSphere.API.Controllers
{
    [Route("api/class")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/<ClassController>
        [HttpPost]
        public async Task<IActionResult> StaffCreateClass([FromBody] CreateClassCommand command, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorCount);
            }

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

        [HttpPost("imports")]
        public async Task<IActionResult> StaffImportExcel(IFormFile file)
        {
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Must be Excel file.");
            }

            var command = new ImportClassCommand();

            // Parse Classes data from file
            try
            {
                var parsedClasses = await FileParser.ParseClassFromExcel(file.OpenReadStream());
                command.Classes = parsedClasses;
            }
            catch (Exception)
            {
                return BadRequest("Invalid data from file.");
            }

            // Send command
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

        // Roles: Staff
        [HttpGet]
        public async Task<IActionResult> StaffGetAllClasses(GetAllClassesQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.PaginatedClasses);
        }

        // Roles: Staff, Lecturer, Student
        [Authorize(Roles = "3, 4, 5")]
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassDetailsById(GetClassByIdQuery query, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            query.ViewerUId = int.Parse(UIdClaim.Value);
            query.ViewerRole = int.Parse(roleClaim.Value);

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            if (!result.Authorized)
            {
                return Unauthorized(result.Message);
            }

            if (result.Class == null)
            {
                return NotFound();
            }

            return Ok(result.Class);
        }

        // Roles: Lecturer
        //[Authorize(Roles = "4")]
        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetLecturerClasses(GetLecturerClassesQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.PaginatedClasses);
        }

        // Roles: Student
        //[Authorize(Roles = "5")]
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentClasses(GetStudentClassesQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.PaginatedClasses);
        }

        [Authorize]
        [HttpPatch("{classId}/lecturer-assignment")]
        public async Task<IActionResult> AssignLecturerToClass(AssignLecturerToClassCommand command)
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

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("{classId}/students")]
        public async Task<IActionResult> AddStudentToClass(int classId, [FromBody] AddStudentToClassCommand command)
        {
            if (classId != command.ClassId)
            {
                return BadRequest("ClassId in url route and body do not match.");
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

        [Authorize(Roles = "2, 4")] // Roles: HeadDepartment, Lecturer
        [HttpPatch("{classId}/projects-assignment")]
        public async Task<IActionResult> AssignProjectsToClass(int classId, AssignProjectsToClassCommand command, CancellationToken cancellationToken = default)
        {
            // Get UserId & Role of requester
            var UIdClaim = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = User.Claims.First(c => c.Type == ClaimTypes.Role);
            command.UserId = int.Parse(UIdClaim.Value);
            command.Role = int.Parse(roleClaim.Value);
            command.ClassId = classId;

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
