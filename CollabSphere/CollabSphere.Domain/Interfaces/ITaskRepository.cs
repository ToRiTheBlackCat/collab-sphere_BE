using CollabSphere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = CollabSphere.Domain.Models.Task;

namespace CollabSphere.Domain.Interfaces
{
    public interface ITaskRepository : IGenericRepository<Task>
    {
    }
}
