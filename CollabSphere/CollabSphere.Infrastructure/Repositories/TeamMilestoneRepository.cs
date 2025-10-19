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

        public override async Task<TeamMilestone?> GetById(int id)
        {
            var milestone = await _context.TeamMilestones
                .AsNoTracking()
                // Team Info for checking viewing user
                .Include(mst => mst.Team)
                    .ThenInclude(team => team.Class)
                .Include(mst => mst.Team)
                    .ThenInclude(team => team.ClassMembers)
                // Milestone questions Info
                .Include(mst => mst.MilestoneQuestions)
                    .ThenInclude(question => question.MilestoneQuestionAns)
                // Checkpoints Info
                .Include(mst => mst.Checkpoints)
                    .ThenInclude(check => check.CheckpointAssignments)
                        .ThenInclude(assign => assign.ClassMember)
                            .ThenInclude(member => member.Student)
                .Include(mst => mst.Checkpoints)
                    .ThenInclude(check => check.CheckpointFiles)
                .FirstOrDefaultAsync(mst => mst.TeamMilestoneId == id);

            return milestone;
        }

        public async Task<List<TeamMilestone>> GetMilestonesByTeamId(int teamId)
        {
            var query = _context.TeamMilestones
                .AsNoTracking()
                .Where(x => x.TeamId == teamId)
                .Include(x => x.Checkpoints)
                    .ThenInclude(x => x.CheckpointAssignments)
                        .ThenInclude(x => x.ClassMember)
                            .ThenInclude(x => x.Student)
                .Include(x => x.Checkpoints)
                    .ThenInclude(x => x.CheckpointFiles)
                .Include(x => x.MilestoneQuestions)
                .OrderBy(mile => mile.StartDate)
                .ToListAsync();

            var milestones = await query;
            if (milestones.Any())
            {
                foreach (var milestone in milestones)
                {
                    milestone.Checkpoints = milestone.Checkpoints.OrderBy(x => x.StartDate).ToList();
                }
            }

            return milestones;
        }
    }
}
