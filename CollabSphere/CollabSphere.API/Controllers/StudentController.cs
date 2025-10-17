using CollabSphere.Application.Common;
using CollabSphere.Application.Features.Student.Commands.ImportStudent;
using CollabSphere.Application.Features.Student.Queries.GetAllStudent;
using CollabSphere.Application.Features.Student.Queries.GetStudentOfClass;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollabSphere.API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IExcelFormatValidator _excelValidate;

        public StudentController(IMediator mediator, IExcelFormatValidator excelValidate)
        {
            _mediator = mediator;
            _excelValidate = excelValidate;
        }

        [HttpPost("imports")]
        public async Task<IActionResult> ImportFileToCreateStudents(IFormFile file)
        {
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Must be Excel file.");
            }

            var isValid = _excelValidate.ValidateTableHeaderFormat(file.OpenReadStream(), "STUDENT");
            if (!isValid)
            {
                return BadRequest("Invalid header row format. Please view sample format header to import data!");
            }

            var command = new ImportStudentCommand();

            try
            {
                var parsedStudents = await FileParser.ParseListStudentFromExcel(file.OpenReadStream());
                command.StudentList = parsedStudents;
            }
            catch (Exception)
            {
                return BadRequest("Invalid data from file.");
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetALlStudents(GetAllStudentQuery query, CancellationToken cancellationToken = default)
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

            return Ok(result.PaginatedStudents);
        }

        [Authorize] 
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetStudentsOfClass(GetStudentOfClassQuery query, CancellationToken cancellationToken = default)
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
