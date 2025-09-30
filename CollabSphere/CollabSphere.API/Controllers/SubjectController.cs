using CollabSphere.Application.Features.Subjects.Commands.CreateSubject;
using CollabSphere.Application.Features.Subjects.Commands.UpdateSubject;
using CollabSphere.Application.Features.Subjects.Queries.GetAllSubject;
using CollabSphere.Application.Features.Subjects.Queries.GetSubjectById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CollabSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SubjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/<SubjectController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new GetAllSubjectsQuery());

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Subjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _mediator.Send(new GetSubjectByIdQuery()
            {
                SubjectId = id
            });

            if (!result.IsValidInput)
            {
                return BadRequest(result);
            }

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result.Subject);
        }

        [HttpPost("api/academic/subject")]
        public async Task<IActionResult> AcademicCreateSubject([FromBody] CreateSubjectCommand command)
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
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result);
        }

        [HttpPut("api/academic/subject")]
        public async Task<IActionResult> AcademicUpdateSubject([FromBody] UpdateSubjectCommand command)
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
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }

            return Ok(result);
        }
    }
}
