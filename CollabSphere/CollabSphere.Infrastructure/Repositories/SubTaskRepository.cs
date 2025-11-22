using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class SubTaskRepository : GenericRepository<SubTask>, ISubTaskRepository
    {
        public SubTaskRepository(collab_sphereContext context) : base(context)
        {
        }
    }
}
