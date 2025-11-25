using CollabSphere.Application.DTOs.ProjectRepos;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ProjectRepos
{
    public class MappedProjectRepositoryVM
    {
        public long GithubInstallationId { get; set; }

        public long RepositoryId { get; set; }

        public string RepositoryFullName { get; set; } = null!;

        public int TeamId { get; set; }

        public string TeamName { get; set; } = null!;

        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = null!;
    }
}

namespace CollabSphere.Application.Mappings.ProjectRepoMappings
{
    public static class ProjectRepoMappingMappings
    {
        public static MappedProjectRepositoryVM ToViewModel(this ProjectRepoMapping repoMapping)
        {
            return new MappedProjectRepositoryVM()
            {
                GithubInstallationId = repoMapping.GithubInstallationId,
                RepositoryId = repoMapping.RepositoryId,
                RepositoryFullName = repoMapping.RepositoryFullName,
                TeamId = repoMapping.TeamId,
                TeamName = repoMapping.Team?.TeamName ?? "NOT_FOUND",
                ProjectId = repoMapping.ProjectId,
                ProjectName = repoMapping.Project?.ProjectName ?? "NOT_FOUND",
            };
        }

        public static List<MappedProjectRepositoryVM> ToViewModels(this IEnumerable<ProjectRepoMapping> repoMappings)
        {
            if (repoMappings == null || !repoMappings.Any())
            {
                return new List<MappedProjectRepositoryVM> { };
            }

            return repoMappings.Select(x => x.ToViewModel()).ToList();
        }
    } 
}
