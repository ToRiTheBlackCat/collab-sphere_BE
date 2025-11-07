using CollabSphere.Application.DTOs.ProjectRepo;
using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.ProjectRepos
{
    public static class ProjectRepoMapping
    {
        public static List<AllReposOfProjectDto> ListRepo_To_ListAllReposOfProjectDto(this List<Domain.Entities.ProjectRepository>? list)
        {
            if (list.Any() == false || list == null)
            {
                return new List<AllReposOfProjectDto>();
            }


            var dtoList = new List<AllReposOfProjectDto>();
            foreach (var repo in list)
            {
                if (repo == null)
                {
                    continue;
                }

                var dto = new AllReposOfProjectDto
                {
                    Id = repo.Id,
                    ProjectId = repo.Id,
                    GithubRepoId = repo.GithubRepoId,
                    RepositoryName = repo.RepositoryName,
                    RepositoryUrl = repo.RepositoryUrl,
                    WebhookId = repo.WebhookId,
                    ConnectedByUserId = repo.ConnectedByUserId,
                    ConnectedAt = repo.ConnectedAt,
                };
                dtoList.Add(dto);
            }

            return dtoList;
        }
    }
}
