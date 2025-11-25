using CollabSphere.Application.Common;
using CollabSphere.Application.Features.GithubConnectionStates.Commands.GenerateNewInstallationUrl;
using CollabSphere.Application.Features.GithubConnectionStates.Commands.MapReposFromInstallation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Controllers
{
    [Route("api/github-installation")]
    [ApiController]
    public class GithubInstallationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GithubInstallationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetInstallations()
        {
            try
            {
                var config = GithubInstallationHelper.GetInstallationConfig();
                var jwt = GithubInstallationHelper.CreateJwt(config.appId, config.privateKey);

                var test = await GithubInstallationHelper.GetInstallations(jwt);
                return Ok(test);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("repositories")]
        public async Task<IActionResult> GetInstallations(long installationId)
        {
            try
            {
                var config = GithubInstallationHelper.GetInstallationConfig();
                var jwt = GithubInstallationHelper.CreateJwt(config.appId, config.privateKey);

                var test = await GithubInstallationHelper.GetRepositoresByInstallationId(installationId, jwt);
                return Ok(test);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Roles: Student
        [HttpPost("connection")]
        [Authorize(Roles = "5")]
        public async Task<IActionResult> GenerateCreateNewInstallationUrl(GenerateNewInstallationUrlCommand command, CancellationToken cancellationToken = default)
        {
            var jwtUserInfo = this.GetCurrentUserInfo();
            command.UserId = jwtUserInfo.UserId;
            command.UserRole = jwtUserInfo.RoleId;

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

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

        // Roles: Student
        [HttpPost("connection-callback")]
        [Authorize(Roles = "5")]
        public async Task<IActionResult> MapRepositoriesFromInstallation(MapReposFromInstallationCommand command, CancellationToken cancellationToken = default)
        {
            var jwtUserInfo = this.GetCurrentUserInfo();
            command.UserId = jwtUserInfo.UserId;
            command.UserRole = jwtUserInfo.RoleId;

            // Handle command
            var result = await _mediator.Send(command, cancellationToken);

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
