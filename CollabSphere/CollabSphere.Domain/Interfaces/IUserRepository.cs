using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Domain.Intefaces
{
    public interface IUserRepository
    {
        Task<List<User>?> GetAllHeadDepartAsync();
        Task<List<User>?> GetAllStaffAsync();
        Task<List<User>?> GetAllLecturerAsync();
        Task<List<User>?> GetAllStudentAsync();
        Task<User?> GetOneByEmailAndPassword(string email, string password);
        Task<User?> GetOneByEmail(string email);
        Task<User?> GetOneByUIdWithInclude(int uid);
        Task<User?> GetOneByUserIdAsync(int userId);
        Task<User?> GetOneByIdWithIncludeAsync(int TId, string typeId, params Expression<Func<User, dynamic>>[] includeProperties);
        Task<User?> GetUserByClassMemberId(int classMemberId);
        Task InsertUser(User user);
        void UpdateUser(User user);
        Task<User?> GetStudentByStudentCodeAsync(string? studentCode);
        Task<User?> GetLecturerByLecturerCodeAsync(string? lecturerCode);
        Task<User?> GetUserAccountIncludeWithAllStatus(int userId);
    }
}
