using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IMeetingRepository : IGenericRepository<Meeting>
    {
        Task<List<Meeting>?> SearchMeeting(int teamId, string? title, DateTime? scheduleTime, int? status, bool isDesc);
    }
}
