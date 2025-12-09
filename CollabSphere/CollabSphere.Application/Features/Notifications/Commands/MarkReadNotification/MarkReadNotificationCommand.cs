using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Notifications.Commands.MarkReadNotification
{
    public class MarkReadNotificationCommand : ICommand
    {
        public int NotificationId { get; set; } = -1;

        public int UserId = -1;

        public int UserRole = -1;
    }
}
