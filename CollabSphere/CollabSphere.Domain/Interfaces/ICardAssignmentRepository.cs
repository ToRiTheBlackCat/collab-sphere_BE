using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface ICardAssignmentRepository : IGenericRepository<CardAssignment>
    {
        Task<CardAssignment?> GetOneByCardIdAndStuId(int cardId, int studentId);
    }
}
