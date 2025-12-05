using CollabSphere.Application.Constants;
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

        public async Task<TeamMilestone?> GetDetailById(int teamMilestoneId)
        {
            var milestone = await _context.TeamMilestones
                .AsNoTracking()
                // Team Info for checking viewing user
                .Include(mst => mst.Team)
                    .ThenInclude(team => team.Class)
                .Include(mst => mst.Team)
                    .ThenInclude(team => team.ClassMembers)
                        .ThenInclude(member => member.Student)
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
                // Evaluation Info
                .Include(mst => mst.MilestoneEvaluation)
                    .ThenInclude(eval => eval.Lecturer)
                // File Info (For Lecturer)
                .Include(mst => mst.MilestoneFiles)
                    .ThenInclude(file => file.User)
                        .ThenInclude(user => user.Lecturer)
                // Return Info (For Student)
                .Include(mst => mst.MilestoneReturns)
                    .ThenInclude(rtrn => rtrn.User)
                        .ThenInclude(member => member.Student)
                .FirstOrDefaultAsync(mst => 
                    mst.TeamMilestoneId == teamMilestoneId &&
                    mst.Status != (int)TeamMilestoneStatuses.SOFT_DELETED);

            if (milestone != null)
            {
                milestone.Checkpoints = milestone.Checkpoints.OrderBy(x => x.StartDate).ToList();
            }

            return milestone;
        }

        public async Task<List<TeamMilestone>> GetMilestonesByTeamId(int teamId)
        {
            var query = _context.TeamMilestones
                .AsNoTracking()
                .Where(x => 
                    x.TeamId == teamId &&
                    x.Status != (int)TeamMilestoneStatuses.SOFT_DELETED)
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

        public async Task<TeamMilestone?> GetTeamMilestoneById(int teamMilestoneId)
        {
            return await _context.TeamMilestones
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.TeamMilestoneId == teamMilestoneId);
        }
    }
}
