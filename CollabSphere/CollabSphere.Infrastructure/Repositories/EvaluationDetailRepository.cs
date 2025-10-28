using CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeam;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class EvaluationDetailRepository : GenericRepository<EvaluationDetail>, IEvaluationDetailRepository
    {
        public EvaluationDetailRepository(collab_sphereContext context) : base(context)
        {
        }
    }
}
