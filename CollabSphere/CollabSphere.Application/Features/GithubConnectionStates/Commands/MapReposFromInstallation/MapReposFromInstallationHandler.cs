using CloudinaryDotNet.Core;
using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.GithubConnectionStates.Commands.GenerateNewInstallationUrl;
using CollabSphere.Application.Mappings.ProjectRepoMappings;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CollabSphere.Application.Common.GithubInstallationHelper;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.MapReposFromInstallation
{
    public class MapReposFromInstallationHandler : CommandHandler<MapReposFromInstallationCommand, MapReposFromInstallationResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MapReposFromInstallationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<MapReposFromInstallationResult> HandleCommand(MapReposFromInstallationCommand request, CancellationToken cancellationToken)
        {
            var result = new MapReposFromInstallationResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                ErrorList = new List<OperationError>()
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Get connection state
                var connectionState = (await _unitOfWork.GithubConnectionStateRepo.GetByStateToken(request.StateToken))!;

                // Get repositories from installationId
                var configInfo = GithubInstallationHelper.GetInstallationConfig();
                var jwt = GithubInstallationHelper.CreateJwt(configInfo.appId, configInfo.privateKey);
                var gitRepos = await GithubInstallationHelper.GetRepositoresByInstallationId(request.InstallationId, jwt);
                var gitRepositoryIds = gitRepos.Select(x => x.RepositoryId).ToHashSet();

                // Get exising git repository mappings of Team
                var existMaps = await _unitOfWork.ProjectRepoMappingRepo.GetRepoMapsByTeam(connectionState.TeamId);
                //var mappedRepoIds = existMaps.Select(x => x.RepositoryId).ToHashSet();

                // Cancel request if any repository is connected by another team
                var invalidRepos = new List<GithubRepositoryModel>();
                foreach (var gRepo in gitRepos)
                {
                    var existedRepoMapp = await _unitOfWork.ProjectRepoMappingRepo.GetRepomappingByRepository(gRepo.RepositoryId);
                    if (existedRepoMapp != null && existedRepoMapp.TeamId != connectionState.TeamId)
                    {
                        invalidRepos.Add(gRepo);
                    }
                }
                if (invalidRepos.Any())
                {
                    var invalidRepoNames = invalidRepos.Select(x => x.FullName).ToList();
                    result.ErrorList.Add(new OperationError()
                    {
                        Field = nameof(request.InstallationId),
                        Message = $"These repositories are already installed by other teams: {string.Join(", ", invalidRepoNames)}",
                    });

                    result.IsValidInput = false;
                    return result; 
                }

                // Contruct Mappings for found repositories
                var currentTime = DateTime.UtcNow;
                var createdMaps = new List<ProjectRepoMapping>();
                var skippedMaps = new List<ProjectRepoMapping>();
                var unmappedMaps = new List<ProjectRepoMapping>();
                var createdCount = 0;

                // Delete removed repositoriy mappings
                var removedMaps = existMaps.Where(x =>
                    !gitRepositoryIds.Contains(x.RepositoryId) &&
                    x.GithubInstallationId == request.InstallationId // Only remove repositories connected to the installation_id
                );
                foreach (var rmMap in removedMaps)
                {
                    _unitOfWork.ProjectRepoMappingRepo.Delete(rmMap);
                }
                unmappedMaps.AddRange(removedMaps);
                await _unitOfWork.SaveChangesAsync();

                // Map new repositories to DB
                foreach (var repo in gitRepos)
                {
                    // Check for existing mappings
                    var duplicatedMap = existMaps.FirstOrDefault(x => x.RepositoryId == repo.RepositoryId);
                    if (duplicatedMap != null)
                    {
                        skippedMaps.Add(duplicatedMap);
                        continue;
                    }

                    var mapping = new ProjectRepoMapping()
                    {
                        ProjectId = connectionState!.ProjectId,
                        TeamId = connectionState.TeamId,
                        GithubInstallationId = request.InstallationId,
                        RepositoryId = repo.RepositoryId,
                        RepositoryFullName = repo.FullName,
                        InstalledByUserid = connectionState.UserId,
                        InstalledAt = currentTime,
                    };
                    await _unitOfWork.ProjectRepoMappingRepo.Create(mapping);
                    createdMaps.Add(mapping);
                    createdCount++;
                }
                await _unitOfWork.SaveChangesAsync();

                // Delete temporary connection state
                _unitOfWork.GithubConnectionStateRepo.Delete(connectionState!);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Setup for view model cast
                var team = (await _unitOfWork.TeamRepo.GetById(connectionState!.TeamId))!;
                var project = (await _unitOfWork.ProjectRepo.GetById(connectionState!.ProjectId))!;

                createdMaps = createdMaps.Select(map =>
                {
                    return new ProjectRepoMapping()
                    {
                        ProjectId = map.ProjectId,
                        TeamId = map.TeamId,
                        GithubInstallationId = map.GithubInstallationId,
                        RepositoryId = map.RepositoryId,
                        RepositoryFullName = map.RepositoryFullName,
                        InstalledByUserid = map.InstalledByUserid,
                        InstalledAt = map.InstalledAt,
                        Team = team,
                        Project = project
                    };
                })
                .ToList();

                result.MappedRepositories = createdMaps.ToViewModels();
                result.SkippedRepositories = skippedMaps.ToViewModels();
                result.UnmappedRepositories = unmappedMaps.ToViewModels();
                result.Message = $"Connected {createdCount} repositorie(s) to project '{project.ProjectName}'{project.ProjectId} of team '{team.TeamName}'({team.TeamId}).";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, MapReposFromInstallationCommand request)
        {
            var connectionState = await _unitOfWork.GithubConnectionStateRepo.GetByStateToken(request.StateToken);
            if (connectionState == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.StateToken),
                    Message = $"No connection state with StateToken '{request.StateToken}' found.",
                });
                return;
            }
        }
    }
}
