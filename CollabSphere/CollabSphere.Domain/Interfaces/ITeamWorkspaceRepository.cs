using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface ITeamWorkspaceRepository : IGenericRepository<TeamWorkspace>
    {
        Task<TeamWorkspace?> GetOneByTeamId (int teamId);    
    }
}
