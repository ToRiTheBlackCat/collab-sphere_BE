using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
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
    }
}
