using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IProjectRepoMappingRepository : IGenericRepository<ProjectRepoMapping>
    {
        Task<Dictionary<int, List<ProjectRepoMapping>>> SearchInstallationsOfProject(int projectId, int installedUserId, int teamId, DateTime? fromDate, bool isDesc);

        Task<ProjectRepoMapping?> GetOneByTeamIdAndRepoId(int teamId, long repoId);

        Task<List<ProjectRepoMapping>> GetRepoMapsByTeam(int teamId);
    }
}
