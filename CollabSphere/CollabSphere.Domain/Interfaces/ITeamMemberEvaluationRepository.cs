using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface ITeamMemberEvaluationRepository : IGenericRepository<TeamMemEvaluation>
    {
        Task<List<TeamMemEvaluation>?> GetTeamMemEvaluations (int teamId, int? lecturerId, int? classMemberId);
    }
}
