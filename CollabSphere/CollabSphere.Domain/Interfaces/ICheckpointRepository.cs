using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface ICheckpointRepository : IGenericRepository<Checkpoint>
    {
        Task<Checkpoint?> GetCheckpointDetail(int checkpontId);

        Task<List<Checkpoint>> GetCheckpointsByMilestone(int teamMilestoneId);
    }
}
