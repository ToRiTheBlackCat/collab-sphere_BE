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
    public class WhiteboardPageRepository : GenericRepository<WhiteboardPage>, IWhiteboardPageRepository
    {
        public WhiteboardPageRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<WhiteboardPage>?> GetPagesOfWhiteboard(int whiteboardId)
        {
            return await _context.WhiteboardPages
                .AsNoTracking()
                .Where(x => x.WhiteboardId == whiteboardId)
                .ToListAsync();
        }

        public async Task<WhiteboardPage?> GetWithShape(int pageId)
        {
            return await _context.WhiteboardPages
                .Include(x => x.Shapes)
                .FirstOrDefaultAsync(x => x.PageId == pageId);
        }
    }
}
