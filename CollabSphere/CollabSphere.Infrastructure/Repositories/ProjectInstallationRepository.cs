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
    public class ProjectInstallationRepository : GenericRepository<ProjectInstallation>, IProjectInstallationRepository
    {
        public ProjectInstallationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<ProjectInstallation>?> SearchInstallationsOfProject(int projectId, int installedUserId, int teamId, DateTime? fromDate, bool isDesc)
        {
            var query = _context.ProjectInstallations
              .Where(x => x.ProjectId == projectId)
              .Include(x => x.Project)
              .Include(x => x.InstalledByUser)
              .AsQueryable();

            if (installedUserId != 0)
            {
                query = query.Where(x => x.InstalledByUserId == installedUserId);
            }

            if (teamId != 0)
            {
                query = query.Where(x => x.TeamId == teamId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.InstalledAt >= fromDate.Value);
            }

            query = isDesc
                ? query.OrderByDescending(x => x.InstalledAt)
                : query.OrderBy(x => x.InstalledAt);

            return await query.ToListAsync();
        }

    }
}
