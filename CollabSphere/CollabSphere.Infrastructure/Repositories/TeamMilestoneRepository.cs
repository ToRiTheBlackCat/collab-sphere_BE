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
    public class TeamMilestoneRepository : GenericRepository<TeamMilestone>, ITeamMilestoneRepository
    {
        public TeamMilestoneRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<TeamMilestone>> GetMilestonesByTeamId(int teamId)
        {
            var query = _context.TeamMilestones
                .AsNoTracking()
                .Where(x => x.TeamId == teamId)
                .Include(x => x.Checkpoints)
                    .ThenInclude(x => x.CheckpointAssignments)
                .Include(x => x.Checkpoints)
                    .ThenInclude(x => x.CheckpointFiles)
                .Include(x => x.MilestoneQuestions)
                    .ThenInclude(x => x.MilestoneQuestionAns)
                .OrderBy(mile => mile.StartDate)
                .ToListAsync();

            var milestones = await query;
            if (milestones.Any())
            {
                foreach(var milestone in milestones)
                {
                    milestone.Checkpoints = milestone.Checkpoints.OrderBy(x => x.StartDate).ToList();
                }
            }

            return milestones;
        }
    }
}
