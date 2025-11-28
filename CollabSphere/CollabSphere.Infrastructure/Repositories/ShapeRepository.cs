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
    public class ShapeRepository : GenericRepository<Shape>, IShapeRepository
    {
        public ShapeRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<Shape>?> GetShapesOfPage(int pageId)
        {
            return await _context.Shapes
                .AsNoTracking()
                .Where(x => x.PageId == pageId)
                .ToListAsync();
        }
    }
}
