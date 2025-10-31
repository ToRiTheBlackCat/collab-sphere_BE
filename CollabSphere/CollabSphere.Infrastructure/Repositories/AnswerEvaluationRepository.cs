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
    public class AnswerEvaluationRepository : GenericRepository<AnswerEvaluation>, IAnswerEvaluationRepository
    {
        public AnswerEvaluationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<AnswerEvaluation>?> GetAnswerEvaluationsOfAnswer(int answerId)
        {
            return await _context.AnswerEvaluations
                .Where(x => x.MilestoneQuestionAnsId == answerId)
                .ToListAsync();
        }
    }
}
