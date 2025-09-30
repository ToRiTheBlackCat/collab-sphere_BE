using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IUserRepository
    {
        Task<User?> GetOneByEmailAndPassword(string email, string password);
        Task<User?> GetOneByEmail(string email);
        Task<User?> GetOneByUserIdAsync(int userId);
        Task<User?> GetOneByIdWithIncludeAsync(int TId, string typeId, params Expression<Func<User, dynamic>>[] includeProperties);
        Task InsertUser(User user);
        void UpdateUser(User user);
    }
}
