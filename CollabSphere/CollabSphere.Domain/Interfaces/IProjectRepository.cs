using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project?> GetProjectDetail(int projectId);
        Task<List<Project>> SearchProjects(List<int>? lecturerIds = null, List<int>? subjectIds = null);
    }
}
