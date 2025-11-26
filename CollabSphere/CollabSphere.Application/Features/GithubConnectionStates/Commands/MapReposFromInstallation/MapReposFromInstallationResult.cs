using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.ProjectRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.MapReposFromInstallation
{
    public class MapReposFromInstallationResult : CommandResult
    {
        public List<MappedProjectRepositoryVM> MappedRepositories { get; set; } = new List<MappedProjectRepositoryVM>();

        public List<MappedProjectRepositoryVM> SkippedRepositories { get; set; } = new List<MappedProjectRepositoryVM>();
    }
}
