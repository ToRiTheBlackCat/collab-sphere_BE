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
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(collab_sphereContext context) : base(context)
        {
        }

        public override async Task<List<Project>> GetAll()
        {
            var projects = await _context.Projects
                .Include(x => x.Lecturer)
                .Include(x => x.Subject)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return projects;
        }

        public async Task<Project?> GetProjectDetail(int projectId)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Include(x => x.Lecturer)
                .Include(x => x.Subject)
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);

            return project;
        }

        public async Task<List<Project>> SearchProjects(List<int>? lecturerIds = null, List<int>? subjectIds = null)
        {
            var projects = await _context.Projects
                .Where(x =>
                    (lecturerIds == null || lecturerIds.Contains(x.LecturerId)) &&
                    (subjectIds == null || subjectIds.Contains(x.SubjectId))
                )
                .ToListAsync();

            return projects;
        }
    }
}
