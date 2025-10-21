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
    public class CheckpointRepository : GenericRepository<Checkpoint>, ICheckpointRepository
    {
        public CheckpointRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<Checkpoint?> GetCheckpointDetail(int checkpontId)
        {
            var checkpoint = await _context.Checkpoints
                .AsNoTracking()
                .Include(x => x.CheckpointFiles)
                .Include(x => x.CheckpointAssignments)
                    .ThenInclude(assign => assign.ClassMember)
                        .ThenInclude(member => member.Student)
                .Include(x => x.TeamMilestone)
                    .ThenInclude(mls => mls.Team)
                        .ThenInclude(team => team.ClassMembers)
                .FirstOrDefaultAsync(x => x.CheckpointId == checkpontId);

            return checkpoint;
        }
    }
}
