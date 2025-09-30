using CollabSphere.Application.Common;
using CollabSphere.Application.Features.Student.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ValidateTableFormat _excelValidate;

        public StudentController(IMediator mediator, ValidateTableFormat excelValidate)
        {
            _mediator = mediator;
            _excelValidate = excelValidate;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFileToCreateStudents (IFormFile file)
        {
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Must be Excel file.");
            }

            var isValid = _excelValidate.ValidateTableHeaderFormat(file.OpenReadStream(), "LECTURER");
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
    }
}
