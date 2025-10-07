using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IProjectAssignmentRepository : IGenericRepository<ProjectAssignment>
    {
        Task<List<ProjectAssignment>> GetProjectAssignmentsByClassAsync(int classId);
    }
}
