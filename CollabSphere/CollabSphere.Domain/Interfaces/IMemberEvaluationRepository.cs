using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IMemberEvaluationRepository : IGenericRepository<MemberEvaluation>
    {
        Task<MemberEvaluation?> SearchEvaluation(int teamId, int raterId, int receiverId,  string? scoreDetailName);
    }
}
