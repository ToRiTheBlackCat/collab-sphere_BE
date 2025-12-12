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
    public class TeamMemberEvaluationRepository : GenericRepository<TeamMemEvaluation>, ITeamMemberEvaluationRepository
    {
        public TeamMemberEvaluationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<TeamMemEvaluation>?> GetTeamMemEvaluations(int teamId, int? lecturerId, int? classMemberId)
        {
            var query = _context.TeamMemEvaluations.AsQueryable();
            query = query.Where(t => t.TeamId == teamId);

            if (lecturerId.HasValue)
            {
                query = query.Where(t => t.LecturerId == lecturerId.Value);
            }
            if (classMemberId.HasValue)
            {
                query = query.Where(t => t.ClassMemberId == classMemberId.Value);
            }

            return await query.ToListAsync();
        }
    }
}
