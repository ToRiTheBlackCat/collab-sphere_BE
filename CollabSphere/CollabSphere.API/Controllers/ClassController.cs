using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.Features.Classes.Commands.AssignLec;
using CollabSphere.Application.Features.Classes.Commands.CreateClass;
using CollabSphere.Application.Features.Classes.Commands.ImportClass;
using CollabSphere.Application.Features.Classes.Queries.GetAllClasses;
using CollabSphere.Application.Features.Classes.Queries.GetClassById;
using CollabSphere.Application.Features.Classes.Queries.GetLecturerClasses;
using CollabSphere.Application.Features.Classes.Queries.GetStudentClasses;
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

        [HttpPost("import")]
        public async Task<IActionResult> StaffImportExcel(IFormFile file)
        //public async Task<IActionResult> StaffImportExcel(List<ImportClassDto> file)
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

        [Authorize]
        [HttpPatch("{classId}/assign-lecturer")]
        public async Task<IActionResult> AssignLecturerToClass(AssignLecturerToClassCommand command)
        {
            if(!ModelState.IsValid)
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
    }
}
