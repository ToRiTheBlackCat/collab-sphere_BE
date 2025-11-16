using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = CollabSphere.Domain.Entities.Task;

namespace CollabSphere.Domain.Interfaces
{
    public interface ITaskRepository : IGenericRepository<Task>
    {
    }
}
