using CollabSphere.Domain.Entities;
﻿using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<User?> GetOneByEmailAndPassword(string email, string password)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email.Equals(email)
                    && x.Password == password
                    && x.IsActive);
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
