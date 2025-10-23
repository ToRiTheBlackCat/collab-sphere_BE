using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
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
    public class TeamEvaluationRepository : GenericRepository<TeamEvaluation>, ITeamEvaluationRepository
    {
        public TeamEvaluationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<TeamEvaluation?> GetOneByTeamId(int teamId)
        {
            return await _context.TeamEvaluations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TeamId == teamId);
        }
    }
}
