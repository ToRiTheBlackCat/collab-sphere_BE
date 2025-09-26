using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        Task<Subject?> GetBySubjectCode(string subjectCode);
    }
}
