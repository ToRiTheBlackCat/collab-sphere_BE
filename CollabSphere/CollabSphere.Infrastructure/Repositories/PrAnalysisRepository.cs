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
    public class PrAnalysisRepository : GenericRepository<PrAnalysis>, IPrAnalysisRepository
    {
        public PrAnalysisRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<PrAnalysis?> SearchPrAnalysis(int projectId, int teamId, long repositoryId, int prNumber)
        {
            return await _context.PrAnalyses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == projectId &&
                                          x.TeamId == teamId &&
                                          x.RepositoryId == repositoryId &&
                                          x.PrNumber == prNumber);
        }
    }
}
