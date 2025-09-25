
using CollabSphere.Domain.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application
{
    public interface IUnitOfWork : IDisposable
    {
       
        IUserRepository UserRepo { get; }
        IClassMemberRepository ClassMemberRepo { get; }
        IClassRepository ClassRepo { get; }
        ILecturerRepository LecturerRepo { get; }
        IStudentRepository StudentRepo { get; }
        ISubjectRepository SubjectRepo { get; }
        //More IRepo below

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
