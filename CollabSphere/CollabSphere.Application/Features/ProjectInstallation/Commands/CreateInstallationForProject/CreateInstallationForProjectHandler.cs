using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject
{
    public class CreateInstallationForProjectHandler : CommandHandler<CreateInstallationForProjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateInstallationForProjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(CreateInstallationForProjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundProject = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
                if (foundProject != null)
                {
                    var newProjectInstallation = new ProjectInstallation
                    {
                        ProjectId = request.ProjectId,
                        GithubInstallationId = request.InstallationId,
                        TeamId = request.TeamId,
                        InstalledByUserId = request.UserId,
                        InstalledAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ProjectInstallationRepo.Create(newProjectInstallation);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Create installation for project with ID: {request.ProjectId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateInstallationForProjectCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check role permission
            if (bypassRoles.Contains(request.UserRole))
            {
                //Find project
                var foundProject = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
                if (foundProject == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.ProjectId),
                        Message = $"Not found any project with that Id: {request.ProjectId}"
                    });
                    return;
                }

                //Find team
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.ProjectId),
                        Message = $"Not found any project with that Id: {request.TeamId}"
                    });
                    return;
                }
                else
                {
                    //If student
                    if (request.UserRole == RoleConstants.STUDENT)
                    {
                        //Check if team member
                        var foundTeamMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.UserId);
                        if (foundTeamMem == null)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the member of this team. Cannot create project installation for this team"
                            });
                            return;
                        }
                    }
                    //If Lec
                    else
                    {
                        if (request.UserId != foundTeam.LecturerId)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"You are not the lecturer of this team. Cannot create meeting for this team"
                            });
                            return;
                        }
                    }
                }
            }
            else
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserRole),
                    Message = $"You do not have permission to use this function"
                });
                return;
            }
        }
    }
}
