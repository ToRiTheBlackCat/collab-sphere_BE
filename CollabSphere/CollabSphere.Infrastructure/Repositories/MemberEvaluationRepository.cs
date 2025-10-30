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
    public class MemberEvaluationRepository : GenericRepository<MemberEvaluation>, IMemberEvaluationRepository
    {
        public MemberEvaluationRepository(collab_sphereContext context) : base(context) { }

        public async Task<MemberEvaluation?> SearchEvaluation(int teamId, int raterId, int receiverId, string? scoreDetailName)
        {
            var query = _context.MemberEvaluations
                .Where(x => x.TeamId == teamId
                    && x.RaterId == raterId
                    && x.ReceiverId == receiverId);

            if (!string.IsNullOrWhiteSpace(scoreDetailName))
            {
                query = query.Where(x => x.Comment.ToLower().Trim() == scoreDetailName.Trim().ToLower());
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, List<MemberEvaluation>>> GetEvaluationsForReceiver(int teamId, int receiverId)
        {
            return await _context.MemberEvaluations
                .AsNoTracking()
                .Where(x => x.TeamId == teamId && x.ReceiverId == receiverId)
                .GroupBy(x => x.RaterId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }

        public async Task<Dictionary<int, List<MemberEvaluation>>> GetEvaluationsOfOwnByUser(int teamId, int raterId)
        {
            return await _context.MemberEvaluations
                .AsNoTracking()
                .Where(x => x.TeamId == teamId && x.RaterId == raterId)
                .GroupBy(x => x.ReceiverId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }
    }
}
