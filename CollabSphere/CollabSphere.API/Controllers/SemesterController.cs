using CollabSphere.Application.Features.Semesters.Commands.CreateSemester;
using CollabSphere.Application.Features.Semesters.Queries.GetAllSemester;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CollabSphere.API.Controllers
{
    [Route("api/semester")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SemesterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSemester(CancellationToken cancellationToken = default)
        {
            var query = new GetAllSemestersQuery();

            // Handle query
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsValidInput)
            {
                return BadRequest(result.ErrorList);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Semesters);
        }

        [Authorize(Roles = "2")]
        [HttpPost]
        public async Task<IActionResult> CreateSemester([FromBody] CreateSemesterCommand command)
        {
            if (!ModelState.IsValid)

            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

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

        [Authorize(Roles = "2")]
        [HttpPut("{semesterId}")]
        public async Task<IActionResult> UpdateSemester(int semesterId, [FromBody] UpdateSemesterCommand command)
        {
            if (!ModelState.IsValid)

            {
                return BadRequest(ModelState);
            }
            command.SemesterId = semesterId;

            var result = await _mediator.Send(command);

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
    }
}
