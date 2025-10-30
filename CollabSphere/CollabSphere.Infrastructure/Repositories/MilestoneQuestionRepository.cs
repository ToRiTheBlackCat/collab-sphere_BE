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
    public class MilestoneQuestionRepository : GenericRepository<MilestoneQuestion>, IMilestoneQuestionRepository
    {
        public MilestoneQuestionRepository(collab_sphereContext context) : base(context)
        {
        }

        public override async Task<MilestoneQuestion?> GetById (int questionId)
        {
            return await _context.MilestoneQuestions
                .SingleOrDefaultAsync(x => x.MilestoneQuestionId == questionId);
        }
    }
}
