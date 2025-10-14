using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IClassMemberRepository : IGenericRepository<ClassMember>
    {
        Task<List<ClassMember>> GetClassMemberAsyncByClassId(int classId);
        Task<ClassMember?> GetClassMemberAsyncByClassIdAndStudentId(int classId, int studentId);
        Task<List<ClassMember>?> GetClassMemberAsyncByTeamId(int teamId);
    }
}
