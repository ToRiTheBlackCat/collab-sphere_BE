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
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        public ClassRepository(collab_sphereContext context) : base(context) { }

        public override async Task<Class?> GetById(int classId)
        {
            var selectedClass = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Lecturer)
                .Include(x => x.ClassMembers).ThenInclude(x => x.Student)
                .FirstOrDefaultAsync(x =>
                    x.ClassId == classId
                );

            return await selectedClass;
        }

        public async Task<IEnumerable<Class>> GetClassByStudentId(int studentId)
        {
            var classes = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Lecturer)
                .Include(x => x.ClassMembers).ThenInclude(x => x.Student)
                .Where(x =>
                    x.ClassMembers.Where(x => x.StudentId == studentId).Any()
                );

            return await classes.ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetClassByLecturerId(int lecturerId)
        {
            var classes = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Lecturer)
                .Include(x => x.ClassMembers).ThenInclude(x => x.Student)
                .Where(x =>
                    x.Lecturer.LecturerId == lecturerId
                );

            return await classes.ToListAsync();
        }

        public override async Task<List<Class>> GetAll()
        {
            var classes = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Lecturer)
                .Include(x => x.ClassMembers).ThenInclude(x => x.Student)
                .ToListAsync();

            return await classes;
        }
    }
}
