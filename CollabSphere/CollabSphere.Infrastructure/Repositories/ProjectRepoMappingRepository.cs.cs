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
    public class ProjectRepoMappingRepository : GenericRepository<ProjectRepoMapping>, IProjectRepoMappingRepository
    {
        public ProjectRepoMappingRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<Dictionary<int, List<ProjectRepoMapping>>> SearchInstallationsOfProject(int projectId, int installedUserId, int teamId, DateTime? fromDate, bool isDesc)
        {
            var query = _context.ProjectRepoMappings
              .Where(x => x.ProjectId == projectId)
              .Include(x => x.Project)
              .Include(x => x.InstalledByUser)
              .Include(x => x.Team)
              .AsQueryable();

            if (installedUserId != 0)
            {
                query = query.Where(x => x.InstalledByUserid == installedUserId);
            }

            if (teamId != 0)
            {
                query = query.Where(x => x.TeamId == teamId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.InstalledAt >= fromDate.Value);
            }

            var mappings = await query.ToListAsync();

            var groupedResults = mappings
            .GroupBy(x => x.TeamId)
            .ToDictionary(
                g => g.Key,       // The Key is the TeamId
                g => g.ToList()    // The Value is the List<ProjectRepoMapping>
            );

            return groupedResults;
        }

    }
}
