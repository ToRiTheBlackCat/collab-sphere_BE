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
    public class SubjectOutcomeRepository : GenericRepository<SubjectOutcome>, ISubjectOutcomeRepository
    {
        public SubjectOutcomeRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<SubjectOutcome>> GetOutcomesOfSubjectSyllabus(int syllabusId)
        {
            var subjectSyllabus = await _context.SubjectSyllabi
                .AsNoTracking()
                .Include(x => x.SubjectOutcomes)
                    .ThenInclude(outcome => outcome.SyllabusMilestones)
                .FirstOrDefaultAsync(x => x.SyllabusId == syllabusId);

            if (subjectSyllabus == null)
            {
                return new List<SubjectOutcome>();
            }

            return subjectSyllabus.SubjectOutcomes.ToList();
        }
    }
}
