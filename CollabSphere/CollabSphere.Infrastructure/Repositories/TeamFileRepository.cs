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
    public class TeamFileRepository : GenericRepository<TeamFile>, ITeamFileRepository
    {
        public TeamFileRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<TeamFile>> GetFilesByTeam(int teamId)
        {
            var files = await _context.TeamFiles
                .AsNoTracking()
                .Where(x => x.TeamId == teamId)
                .Include(x => x.User)
                    .ThenInclude(user => user.Lecturer)
                .Include(x => x.User)
                    .ThenInclude(user => user.Student)
                .OrderBy(x => x.FilePathPrefix)
                .ThenBy(x => x.FileName)
                .ToListAsync();

            return files;
        }
    }
}
