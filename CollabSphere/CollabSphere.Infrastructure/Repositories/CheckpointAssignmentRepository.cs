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
    public class CheckpointAssignmentRepository : GenericRepository<CheckpointAssignment>, ICheckpointAssignmentRepository
    {
        public CheckpointAssignmentRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<CheckpointAssignment>> GetByCheckpointId(int checkpointId)
        {
            var query = _context.CheckpointAssignments
                .AsNoTracking()
                .Where(x => x.CheckpointId == checkpointId)
                .ToListAsync();

            return await query;
        }
    }
}
