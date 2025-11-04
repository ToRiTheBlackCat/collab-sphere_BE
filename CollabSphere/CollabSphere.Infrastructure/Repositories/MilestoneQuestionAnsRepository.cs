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
    public class MilestoneQuestionAnsRepository : GenericRepository<MilestoneQuestionAn>, IMilestoneQuestionAnsRepository
    {
        public MilestoneQuestionAnsRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<MilestoneQuestionAn>?> GetAnswersOfQuestionByIdAsync(int questionId)
        {
            return await _context.MilestoneQuestionAns
                .AsNoTracking()
                .Where(x => x.MilestoneQuestionId == questionId)
                .ToListAsync();
        }

        public async Task<MilestoneQuestionAn?> GetAnswerById(int answerId)
        {
            return await _context.MilestoneQuestionAns
                .SingleOrDefaultAsync(x => x.MilestoneQuestionAnsId == answerId);
        }
    }
}
