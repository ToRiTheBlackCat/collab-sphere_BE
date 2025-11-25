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
    public class GithubConnectionStateRepository : GenericRepository<GithubConnectionState>, IGithubConnectionStateRepository
    {
        public GithubConnectionStateRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<GithubConnectionState?> GetGithubConnectionState(int projectId, int teamId)
        {
            var state = await _context.GithubConnectionStates
                .FirstOrDefaultAsync(x => 
                    x.ProjectId == projectId &&
                    x.TeamId == teamId);

            return state;
        }
    }
}
