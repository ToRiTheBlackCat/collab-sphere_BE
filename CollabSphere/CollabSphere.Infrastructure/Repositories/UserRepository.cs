using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.PostgreDbContext;
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
    }
}
