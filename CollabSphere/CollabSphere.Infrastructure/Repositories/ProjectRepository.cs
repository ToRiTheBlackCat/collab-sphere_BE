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
                .Include(x => x.Objectives)
                    .ThenInclude(x => x.ObjectiveMilestones)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            if (projects.Any())
            {
                foreach (var project in projects)
                {
                    foreach (var objective in project.Objectives)
                    {
                        if (!objective.ObjectiveMilestones.Any())
                        {
                            continue;
                        }

                        objective.ObjectiveMilestones = objective.ObjectiveMilestones.OrderBy(mile => mile.StartDate).ToList();
                    }

                    project.Objectives = project.Objectives.OrderBy(obj => obj.ObjectiveMilestones.FirstOrDefault()?.StartDate).ToList();
                }
            }

            return projects;
        }

        public override async Task<Project?> GetById(int id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Include(x => x.Lecturer)
                .Include(x => x.Subject)
                .Include(x => x.Objectives)
                    .ThenInclude(x => x.ObjectiveMilestones)
                .FirstOrDefaultAsync(x => x.ProjectId == id);

            if (project != null && project.Objectives.Any())
            {
                foreach (var objective in project.Objectives)
                {
                    if (!objective.ObjectiveMilestones.Any())
                    {
                        continue;
                    }

                    objective.ObjectiveMilestones = objective.ObjectiveMilestones.OrderBy(mile => mile.StartDate).ToList();
                }

                project.Objectives = project.Objectives.OrderBy(obj => obj.ObjectiveMilestones.FirstOrDefault()?.StartDate).ToList();
            }

            return project;
        }
    }
}
