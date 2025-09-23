using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.UserModule.Repos
{
    public interface IUserRepository
    {
        Task<User?> GetOneByEmailAndPassword(string email, string password);
        Task InsertUser(User user);
        void UpdateUser(User user);
    }
}
