using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface ITeamMilestoneRepository : IGenericRepository<TeamMilestone>
    {
        Task<List<TeamMilestone>> GetMilestonesByTeamId(int teamId);
        Task<TeamMilestone?> GetDetailsById(int teamMilestoneId);
        Task<TeamMilestone?> GetTeamMilestoneById (int teamMilestoneId);
        Task<TeamMilestone?> GetDetailsById(int teamMilestoneId);
    }
}
