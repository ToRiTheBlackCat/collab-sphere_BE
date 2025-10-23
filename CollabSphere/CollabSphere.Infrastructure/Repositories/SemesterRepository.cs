using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
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
    public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
    {
        public SemesterRepository(collab_sphereContext context) : base(context)
        {
        }

        public override async Task<List<Semester>> GetAll()
        {
            var semestersQuery = _context.Semesters
                .AsNoTracking() 
                .OrderByDescending(sem => sem.StartDate)
                .ToListAsync();

            return await semestersQuery;
        }
    }
}
