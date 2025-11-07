using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IProjectRepoRepository : IGenericRepository<Domain.Entities.ProjectRepository>
    {
        Task<List<ProjectRepository>?> SearchReposOfProject (int projectId, string? repoName, string? repoUrl, int connectedUserId, DateTime? fromDate, bool isDesc);
    }
}
