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

        public async Task<List<Team>?> SearchTeam(int classId, string? teamName, DateOnly? fromDate, DateOnly? endDate, bool isDesc)
        {
            var query = _context.Teams
                .Where(x => x.ClassId == classId)
                .Include(x => x.ProjectAssignment)
                .ThenInclude(x => x.Project)
                .Include(x => x.ClassMembers)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teamName))
            {
                query = query.Where(x => x.TeamName.ToLower().Contains(teamName.ToLower().Trim()));
            }

            if (fromDate.HasValue && !endDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= fromDate.Value);
            }

            if (!fromDate.HasValue && endDate.HasValue)
            {
                query = query.Where(x => x.EndDate <= endDate.Value);
            }

            if (fromDate.HasValue && endDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= fromDate.Value && x.EndDate <= endDate.Value);
            }

            query = isDesc
                ? query.OrderByDescending(x => x.TeamName)
                : query.OrderBy(x => x.TeamName);

            return await query.ToListAsync();
        }
    }
}
