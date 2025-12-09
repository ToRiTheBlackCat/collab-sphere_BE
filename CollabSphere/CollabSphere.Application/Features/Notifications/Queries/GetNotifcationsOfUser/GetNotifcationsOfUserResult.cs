using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Notifications.Queries.GetNotifcationsOfUser
{
    public class GetNotifcationsOfUserResult : QueryResult
    {
        public PagedList<NotificationDto>? PaginatedNotifications { get; set; }
    }
}
