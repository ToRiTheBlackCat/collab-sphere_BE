using CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeam;
using CollabSphere.Application.Features.User.Commands.SignUpHead_Staff;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<EvaluationDetail>>  GetEvaluationDetailsByTeamEvaluationId(int teamEvaluationId)
        {
            return await _context.EvaluationDetails
                .AsNoTracking()
                .Where(x => x.TeamEvaluationId == teamEvaluationId)
                .ToListAsync();
        }
    }
}
