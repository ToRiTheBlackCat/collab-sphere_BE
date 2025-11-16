using CollabSphere.Domain.Interfaces;
using CollabSphere.Domain.Entities;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CollabSphere.Infrastructure.Repositories
{
    public class CardAssignmentRepository : GenericRepository<CardAssignment>, ICardAssignmentRepository
    {
        public CardAssignmentRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<CardAssignment?> GetOneByCardIdAndStuId(int cardId, int studentId)
        {
            return await _context.CardAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CardId == cardId && 
                                          x.StudentId == studentId);
        }
    }
}
