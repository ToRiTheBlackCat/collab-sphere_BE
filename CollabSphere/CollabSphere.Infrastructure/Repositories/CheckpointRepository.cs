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
                // Lecturer Info
                .Include(e => e.CheckpointFiles)
                    .ThenInclude(file => file.User)
                        .ThenInclude(user => user.Lecturer)
                // Student Info
                .Include(e => e.CheckpointFiles)
                    .ThenInclude(file => file.User)
                        .ThenInclude(user => user.Student)
                // Assignments Info
                .Include(e => e.CheckpointAssignments)
                    .ThenInclude(assign => assign.ClassMember)
                        .ThenInclude(member => member.Student)
                // TeamMilestone Info
                .Include(e => e.TeamMilestone)
                    .ThenInclude(mls => mls.Team)
                        .ThenInclude(team => team.ClassMembers)
                .FirstOrDefaultAsync(e => e.CheckpointId == checkpontId);

            return checkpoint;
        }
    }
}
