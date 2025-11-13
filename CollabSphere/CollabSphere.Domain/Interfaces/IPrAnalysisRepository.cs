using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IPrAnalysisRepository : IGenericRepository<PrAnalysis>
    {
        Task<PrAnalysis?> SearchPrAnalysis(int projectId, int teamId, long repositoryId, int prNumber);
    }
}
