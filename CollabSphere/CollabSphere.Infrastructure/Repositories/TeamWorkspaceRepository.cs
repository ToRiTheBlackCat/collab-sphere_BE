using CollabSphere.Domain.Interfaces;
using CollabSphere.Domain.Models;
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
    public class TeamWorkspaceRepository : GenericRepository<TeamWorkspace>, ITeamWorkspaceRepository
    {
        public TeamWorkspaceRepository(collab_sphereContext context) : base(context)
        {

        }

        public async Task<TeamWorkspace?> GetOneByTeamId(int teamId)
        {
            return await _context.TeamWorkspaces
                .AsNoTracking()
                .Include(x => x.Lists)
                    .ThenInclude(x => x.Cards)
                        .ThenInclude(x => x.Tasks)
                        .ThenInclude(x => x.SubTasks)
                .Include(x => x.Lists)
                    .ThenInclude(x => x.Cards)
                        .ThenInclude(x => x.CardAssignments)
                .AsSplitQuery()
                .SingleOrDefaultAsync(x => x.TeamId == teamId);
        }
    }
}
