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
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(collab_sphereContext context) : base(context) { }

        public async Task<List<Team>?> GetListTeamOfStudent(int studentId, string? teamName, int? classId)
        {
            var teamQuery = _context.Teams
                .Include(x => x.Class)
                .Include(x => x.ProjectAssignment)
                .ThenInclude(x => x.Project)
                .AsNoTracking()
                .AsQueryable();

            // Filter by team name if provided
            if (!string.IsNullOrEmpty(teamName))
            {
                teamQuery = teamQuery.Where(x => x.TeamName.ToLower().Contains(teamName.ToLower().Trim()));
            }

            //Get class of student that student is member
            var classList = _context.ClassMembers
                .Where(x => x.StudentId == studentId && x.TeamId != null)
                .AsNoTracking();

            // Filter by classId if provided
            if (classId.HasValue)
            {
                classList = classList.Where(x => x.ClassId == classId.Value);
            }

            //Get teams of student
            teamQuery = from team in teamQuery
                        join cls in classList
                        on team.TeamId equals cls.TeamId
                        select team;

            var result = await teamQuery.ToListAsync();
            return result;
        }
    }
}
