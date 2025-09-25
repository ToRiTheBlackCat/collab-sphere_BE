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
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        public SubjectRepository(collab_sphereContext context):base(context) { }

        public override async Task<Subject?> GetById(int id)
        {
            var subject = await _context.Subjects
                .Include(x => x.SubjectSyllabi)
                    .ThenInclude(x => x.SubjectGradeComponents)
                .Include(x => x.SubjectSyllabi)
                    .ThenInclude(x => x.SubjectOutcomes)
                .FirstOrDefaultAsync(x => x.SubjectId == id);
            if (subject != null)
            {
                _context.Entry(subject).State = EntityState.Detached;
            }

            return subject;
        }
    }
}
