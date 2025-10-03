using CollabSphere.Application.Constants;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly collab_sphereContext _context;
        public UserRepository(collab_sphereContext context)
        {
            _context = context;
        }

        public async Task<List<User>?> GetAllHeadDepartAsync()
        {
            return await _context.Users
                .Include(x => x.Role)
                .Where(x => x.RoleId == RoleConstants.HEAD_DEPARTMENT)
                .ToListAsync();
        }

        public async Task<List<User>?> GetAllStaffAsync()
        {
            return await _context.Users
                .Include(x => x.Role)
                .Where(x => x.RoleId == RoleConstants.STAFF)
                .ToListAsync();
        }

        public async Task<List<User>?> GetAllLecturerAsync()
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Lecturer)
                .Where(x => x.RoleId == RoleConstants.LECTURER)
                .ToListAsync();
        }

        public async Task<List<User>?> GetAllStudentAsync()
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Student)
                .Where(x => x.RoleId == RoleConstants.STUDENT)
                .ToListAsync();
        }

        public async Task<User?> GetOneByEmailAndPassword(string email, string password)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email.Equals(email)
                    && x.Password == password
                    && x.IsActive);
        }

        public async Task<User?> GetOneByEmail(string email)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email.Equals(email)
                    && x.IsActive);
        }

        public async Task<User?> GetOneByUId(int uid)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Student)
                .Include(x => x.Lecturer)
                .FirstOrDefaultAsync(x => x.UId == uid
                    && x.IsActive);
        }
        public async Task<User?> GetOneByIdWithIncludeAsync(int TId, string typeId, params Expression<Func<User, dynamic>>[] includeProperties)
        {
            IQueryable<User> query = _context.Set<User>();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, typeId) == TId);
        }

        public async Task<User?> GetOneByUserIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UId == userId && x.IsActive);
        }

        public async Task InsertUser(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
        }


    }
}
