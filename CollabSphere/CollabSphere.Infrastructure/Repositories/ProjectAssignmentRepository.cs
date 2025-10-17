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

        public void DeleteById(int projectAssignmentId)
        {
            var entity = new ProjectAssignment { ProjectAssignmentId = projectAssignmentId };
            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
        }

        public async Task<List<ProjectAssignment>> GetProjectAssignmentsByClassAsync(int classId)
        {
            var projectAssignments = await _context.ProjectAssignments
                .AsNoTracking()
                .Include(x => x.Project)
                    .ThenInclude(x => x.Lecturer)
                .Include(x => x.Project)
                    .ThenInclude(x => x.Subject)
                //.Include(x => x.Class)
                .Where(x => 
                    x.ClassId == classId)
                .ToListAsync();

            return projectAssignments;
        }
    }
}
