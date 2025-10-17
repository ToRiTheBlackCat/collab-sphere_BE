using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Application.Features.Lecturer.Commands.ImportLecturer;
using CollabSphere.Application.Features.Lecturer.Queries.GetAllLec;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Controllers
{
    [Route("api/lecturer")]
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

        [HttpPost("imports")]
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllLecturer(GetAllLecturerQuery query, CancellationToken cancellationToken = default)
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

            return Ok(result.PaginatedLecturers);
        }

    }
}
