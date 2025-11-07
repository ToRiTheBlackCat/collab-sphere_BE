using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class ProjectRepoRepository : GenericRepository<Domain.Entities.ProjectRepository>, IProjectRepoRepository
    {
        public ProjectRepoRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<Domain.Entities.ProjectRepository>?> SearchReposOfProject(int projectId, string? repoName, string? repoUrl, int connectedUserId, DateTime? fromDate, bool isDesc)
        {
            var query = _context.ProjectRepositories
                .Where(x => x.ProjectId == projectId)
                .Include(x => x.Project)
                .Include(x => x.ConnectedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(repoName))
            {
                query = query.Where(x => x.RepositoryName.ToLower().Contains(repoName.ToLower().Trim()));
            }

            if (!string.IsNullOrEmpty(repoUrl))
            {
                query = query.Where(x => x.RepositoryUrl.ToLower().Contains(repoUrl.ToLower().Trim()));
            }

            if (connectedUserId != 0)
            {
                query = query.Where(x => x.ConnectedByUserId == connectedUserId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.ConnectedAt >= fromDate.Value);
            }

            query = isDesc
                ? query.OrderByDescending(x => x.RepositoryName)
                : query.OrderBy(x => x.RepositoryName);

            return await query.ToListAsync();
        }
    }
}
