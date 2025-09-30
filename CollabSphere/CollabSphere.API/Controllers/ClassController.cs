using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.Features.Classes.Commands.CreateClass;
using CollabSphere.Application.Features.Classes.Commands.ImportClass;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/<ClassController>
        [HttpPost("/api/staff/class")]
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

        [HttpPost("/api/staff/class/import")]
        public async Task<IActionResult> StaffImportExcel(IFormFile file)
        //public async Task<IActionResult> StaffImportExcel(List<ImportClassDto> file)
        {
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Must be Excel file.");
            }

            var parsedClasses = await FileParser.ParseClassFromExcel(file.OpenReadStream());
            var request = new ImportClassCommand()
            {
                Classes = parsedClasses,
            };
            var result = await _mediator.Send(request);

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
