using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Domain.Interfaces;
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
    public class NotificationRecipientRepository : GenericRepository<NotificationRecipient>, INotificationRecipientRepository
    {
        public NotificationRecipientRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<NotificationRecipient?> GetNotificationRecipient(int notificationId, int userId)
        {
            var notificationRecipient = await _context.NotificationRecipients
                .AsNoTracking()
                .SingleOrDefaultAsync(x => 
                    x.NotificationId == notificationId &&
                    x.ReceiverId == userId);

            return notificationRecipient;
        }
    }
}
