using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.GenerateNewInstallationUrl
{
    public class GenerateNewInstallationUrlHandler : CommandHandler<GenerateNewInstallationUrlCommand, GenerateNewInstallationUrlResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GenerateNewInstallationUrlHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GenerateNewInstallationUrlResult> HandleCommand(GenerateNewInstallationUrlCommand request, CancellationToken cancellationToken)
        {
            var result = new GenerateNewInstallationUrlResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                // Get unused installations
                var existedSate = await _unitOfWork.GithubConnectionStateRepo
                    .GetGithubConnectionState(request.ProjectId, request.TeamId, request.UserId);

                // URL string builder function
                var urlBuilder = (string stakeToken) => $"https://github.com/apps/collabsphere-ai-reviewer/installations/new?state={stakeToken}";

                // Create new if no existing state
                if (existedSate == null)
                {
                    await _unitOfWork.BeginTransactionAsync();

                    #region Data operation
                    var newState = new GithubConnectionState()
                    {
                        StateToken = Guid.NewGuid().ToString(),
                        TeamId = request.TeamId,
                        ProjectId = request.ProjectId,
                        UserId = request.UserId,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _unitOfWork.GithubConnectionStateRepo.Create(newState);
                    await _unitOfWork.SaveChangesAsync();
                    #endregion

                    await _unitOfWork.CommitTransactionAsync();

                    result.StateToken = newState.StateToken;
                    result.GeneratedUrl = urlBuilder(newState.StateToken);
                }
                // Return existing state
                else
                {
                    result.StateToken = existedSate.StateToken;
                    result.GeneratedUrl = urlBuilder(existedSate.StateToken);
                }

                //var config = GithubInstallationHelper.GetInstallationConfig();
                //result.Jwt = GithubInstallationHelper.CreateJwt(config.appId, config.privateKey);

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GenerateNewInstallationUrlCommand request)
        {
            // Check team exist
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"No Team with ID '{request.TeamId}' found.",
                });
                return;
            }

            // Requester is Lecturer
            if (request.UserRole == RoleConstants.LECTURER)
            {
                // Check if is class's assigned lecturer
                if (request.UserId != team.Class.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{team.Class.ClassId}'.",
                    });
                    return;
                }
            }
            // Requester is Student
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                // Check if is member of team
                var isTeamMember = team.ClassMembers.Any(x => x.StudentId == request.UserId);
                if (!isTeamMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{team.TeamId}'.",
                    });
                    return;
                }
            }

            // Check if project is valid
            var project = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
            if (project == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ProjectId),
                    Message = $"No project with ID '{request.ProjectId}' found.",
                });
                return;
            }

            if (team.ProjectAssignment.ProjectId != project.ProjectId)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ProjectId),
                    Message = $"The project '{project.ProjectName}'({project.ProjectId}) is not the assigned project of team '{team.TeamName}'({team.TeamId}).",
                });
                return;
            }


        }
    }
}
