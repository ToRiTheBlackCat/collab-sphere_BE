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
    public class ClassMemberRepositiory : GenericRepository<ClassMember>, IClassMemberRepository
    {
        public ClassMemberRepositiory(collab_sphereContext context): base(context) {
        
        }

        public override async Task<List<ClassMember>> GetAll()
        {
            return await _context.ClassMembers
                .AsNoTracking()
                .Include(x => x.Student)
                .ToListAsync();
        }
    }
}
