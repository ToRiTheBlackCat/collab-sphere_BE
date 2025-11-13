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
    public class CreateProjectRepoMappingHandler : CommandHandler<CreateProjectRepoMappingCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateProjectRepoMappingHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(CreateProjectRepoMappingCommand request, CancellationToken cancellationToken)
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

                //Get the list of repo
                var requestRepos = request.Repositories;
                if (requestRepos.Count > 0)
                {
                    foreach (var repo in requestRepos)
                    {
                        //Create new repo
                        var newRepo = new ProjectRepoMapping
                        {
                            ProjectId = request.ProjectId,
                            TeamId = request.TeamId,
                            GithubInstallationId = request.InstallationId,
                            RepositoryFullName = repo.RepositoryFullName,
                            RepositoryId = repo.RepositoryId,
                            InstalledByUserid = request.UserId,
                            InstalledAt = DateTime.UtcNow,
                        };

                        await _unitOfWork.ProjectRepoMappingRepo.Create(newRepo);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    await _unitOfWork.CommitTransactionAsync();
                    result.IsSuccess = true;
                    result.Message = $"Create {request.Repositories.Count} repos mapping for project with ID: {request.ProjectId} | team with ID: {request.TeamId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = $"Fail to create project repo mapping. Error detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateProjectRepoMappingCommand request)
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
