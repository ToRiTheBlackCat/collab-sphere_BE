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
    public class MilestoneEvaluationRepository : GenericRepository<MilestoneEvaluation>, IMilestoneEvaluationRepository
    {
        public MilestoneEvaluationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<MilestoneEvaluation?> GetEvaluationOfMilestone(int teamMilestoneId, int lecturerId, int teamId)
        {
            return await _context.MilestoneEvaluations
                .FirstOrDefaultAsync(x => x.MilestoneId == teamMilestoneId &&
                                          x.LecturerId == lecturerId &&
                                          x.TeamId == teamId);
        }
    }
}
