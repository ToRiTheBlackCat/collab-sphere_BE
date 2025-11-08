using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IProjectInstallationRepository : IGenericRepository<ProjectInstallation>
    {
        Task<List<ProjectInstallation>?> SearchInstallationsOfProject (int projectId, string? repoName, string? repoUrl, int connectedUserId, DateTime? fromDate, bool isDesc);
    }
}
