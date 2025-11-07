using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class MeetingRepository : GenericRepository<Meeting>, IMeetingRepository
    {
        public MeetingRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<Meeting>?> SearchMeeting(int teamId, string? title, DateTime? scheduleTime, int? status, bool isDesc)
        {
            var query = _context.Meetings
                .Where(x => x.TeamId == teamId)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.ToLower().Contains(title.ToLower()));
            }

            if(scheduleTime != null)
            {
                query = query.Where(x => x.ScheduleTime >= scheduleTime.Value);
            }

            if(status != null)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            query = isDesc
                ? query.OrderByDescending(x => x.ScheduleTime)
                : query.OrderBy(x => x.ScheduleTime);

            return await query.ToListAsync();
        }
    }
}
