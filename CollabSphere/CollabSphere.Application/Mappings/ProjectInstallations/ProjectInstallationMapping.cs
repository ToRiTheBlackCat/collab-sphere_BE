using CollabSphere.Application.DTOs.ProjectRepo;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.ProjectRepos
{
    public static class ProjectInstallationMapping
    {
        public static List<AllInstallationsOfProjectDto> ListInstallations_To_ListAllInstallationsOfProjectDto(this List<ProjectInstallation>? list)
        {
            if (list.Any() == false || list == null)
            {
                return new List<AllInstallationsOfProjectDto>();
            }


            var dtoList = new List<AllInstallationsOfProjectDto>();
            foreach (var install in list)
            {
                if (install == null)
                {
                    continue;
                }

                var dto = new AllInstallationsOfProjectDto
                {
                    Id = install.Id,
                    ProjectId = install.Id,
                    GithubInstallationId = install.GithubInstallationId,
                    TeamId = install.TeamId,
                    InstallatedByUserId = install.InstalledByUserId,
                    InstalledAt = install.InstalledAt
                };
                dtoList.Add(dto);
            }

            return dtoList;
        }
    }
}
