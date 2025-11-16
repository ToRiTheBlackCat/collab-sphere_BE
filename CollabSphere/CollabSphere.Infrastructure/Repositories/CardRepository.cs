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
    public class CardRepository : GenericRepository<Card>, ICardRepository
    {
        public CardRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<Card?> GetCardDetailByIdWithAllRelativeInfo(int cardId)
        {
            return await _context.Cards
                .AsNoTracking()
                .Include(x => x.Tasks)
                    .ThenInclude(x => x.SubTasks)
                .Include(x => x.CardAssignments)
                .FirstOrDefaultAsync(x => x.CardId == cardId);
        }
    }
}
