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
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetNotificationsOfUser(int userId)
        {
            var notifications = await _context.NotificationRecipients
                .AsNoTracking()
                .Include(x => x.Notification)
                .Where(x => x.ReceiverId == userId)
                .Select(x => x.Notification)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return notifications;
        }
    }
}
