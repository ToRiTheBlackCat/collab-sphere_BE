using CollabSphere.Application.Features.Team.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
﻿using CollabSphere.Application.Features.Project.Commands.DenyProject;
using CollabSphere.Application.Features.Team.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
﻿using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Application.Features.Team.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [Route("api/team")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateTeam ([FromBody] CreateTeamCommand command)
        {
            if(!ModelState.IsValid)

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

        [Authorize]
        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeam(int teamId, [FromBody] UpdateTeamCommand command)
        {
            if (teamId != command.TeamId)
            {
                return BadRequest("Team ID in the URL does not match Team ID in the body.");
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
        
        [Authorize]
        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(DeleteTeamCommand command, CancellationToken cancellationToken = default!)
        {
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
    }
}
