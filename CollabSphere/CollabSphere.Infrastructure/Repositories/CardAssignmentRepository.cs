using CollabSphere.Domain.Interfaces;
using CollabSphere.Domain.Models;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class CardAssignmentRepository : GenericRepository<CardAssignment>, ICardAssignmentRepository
    {
        public CardAssignmentRepository(collab_sphereContext context) : base(context)
        {
        }
    }
}
