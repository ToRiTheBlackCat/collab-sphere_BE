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
    public class SubjectGradeComponentRepository : GenericRepository<SubjectGradeComponent>, ISubjectGradeComponentRepository
    {
        public SubjectGradeComponentRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<SubjectGradeComponent>?> GetComponentsBySubjectId (int subjectId)
        {
            return await _context.SubjectGradeComponents
                .AsNoTracking()
                .Include(x => x.Syllabus)
                .Where(x => x.Syllabus.SubjectId == subjectId)
                .ToListAsync();
        }
    }
}
