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
    public class ClassFileRepository : GenericRepository<ClassFile>, IClassFileRepository
    {
        public ClassFileRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<ClassFile>> GetFilesByClass(int classId)
        {
            var classFiles = await _context.ClassFiles
                .AsNoTracking()
                .Include(x => x.User)
                    .ThenInclude(user => user.Lecturer)
                .Where(x => x.ClassId == classId)
                .ToListAsync();

            return classFiles;
        }
    }
}
