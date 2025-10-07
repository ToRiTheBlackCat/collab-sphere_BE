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
    public class ProjectAssignmentRepository : GenericRepository<ProjectAssignment>, IProjectAssignmentRepository
    {
        public ProjectAssignmentRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<ProjectAssignment>> GetProjectAssignmentsByClassAsync(int classId)
        {
            var projectAssignments = await _context.ProjectAssignments
                .Include(x => x.Project)
                    .ThenInclude(x => x.Lecturer)
                .Include(x => x.Project)
                    .ThenInclude(x => x.Subject)
                .Include(x => x.Class)
                .AsNoTracking()
                .Where(x => 
                    x.ClassId == classId)
                .ToListAsync();

            return projectAssignments;
        }
    }
}
