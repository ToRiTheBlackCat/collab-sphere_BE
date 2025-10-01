using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Features.Lecturer.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IExcelFormatValidator _excelValidate;

        public LecturerController(IMediator mediator, IExcelFormatValidator excelValidate)
        {
            _mediator = mediator;
            _excelValidate = excelValidate;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFileToCreateLecturers(IFormFile file)
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

            var command = new ImportLecturerCommand();

            try
            {
                var parsedLecturers = await FileParser.ParseListLecturerFromExcel(file.OpenReadStream());
                command.LecturerList = parsedLecturers;
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
