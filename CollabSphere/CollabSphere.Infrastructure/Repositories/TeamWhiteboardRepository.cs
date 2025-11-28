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
    public class TeamWhiteboardRepository : GenericRepository<TeamWhiteboard>, ITeamWhiteboardRepository
    {
        public TeamWhiteboardRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<TeamWhiteboard?> GetByTeamId(int teamId)
        {
            return await _context.TeamWhiteboards
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TeamId == teamId);
        }
    }
}
