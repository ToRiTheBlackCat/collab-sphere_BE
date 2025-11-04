using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IMilestoneEvaluationRepository : IGenericRepository<MilestoneEvaluation>
    {
        Task<MilestoneEvaluation?> GetEvaluationOfMilestone(int teamMilestoneId, int lecturerId, int teamId);
    }
}
