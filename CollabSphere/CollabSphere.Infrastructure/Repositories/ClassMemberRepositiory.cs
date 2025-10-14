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

        public async Task<List<ClassMember>> GetClassMemberAsyncByClassId(int classId)
        {
            return await _context.ClassMembers
                .AsNoTracking()
                .Where(x => x.ClassId == classId)
                .ToListAsync();
        }

        public async Task<ClassMember?> GetClassMemberAsyncByClassIdAndStudentId(int classId, int studentId)
        {
            return await _context.ClassMembers
                .AsNoTracking()
                .Where(x => x.ClassId == classId && x.StudentId == studentId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ClassMember>?> GetClassMemberAsyncByTeamId(int teamId)
        {
            return await _context.ClassMembers
                .AsNoTracking()
                .Where(x => x.TeamId == teamId)
                .ToListAsync();
        }
    }
}
