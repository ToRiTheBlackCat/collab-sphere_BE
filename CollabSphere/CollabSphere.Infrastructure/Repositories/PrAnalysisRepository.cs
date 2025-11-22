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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<(int,List<PrAnalysis>?)> GetListOfAnalysisByTeamIdAndRepoId(int teamId, long repositoryId, int currentPage, int pageSize, bool isDesc)
        {
            var query = _context.PrAnalyses
                .AsNoTracking()
                .Include(x => x.Team)
                .Include(x => x.Project)
                .Where(x => x.TeamId == teamId &&
                x.RepositoryId == repositoryId);
            int totalCount = await query.CountAsync();

            query = isDesc
             ? query.OrderByDescending(x => x.Id) 
             : query.OrderBy(x => x.Id);

            var list = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (totalCount, list);
        }
    }
}
