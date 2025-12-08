using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string NotificationType { get; set; }

        public int? ReferenceId { get; set; }

        public string ReferenceType { get; set; }

        public string Link { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? ReadAt { get; set; }
    }

    public static partial class NotificationMappings
    {
        public static NotificationDto ToNotificationDto(this Notification notification, int? userId = null)
        {
            NotificationRecipient? notiRecipient = null;
            if (userId.HasValue)
            {
                notiRecipient = notification.NotificationRecipients
                    .FirstOrDefault(x => x.ReceiverId == userId);
            }

            return new NotificationDto()
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Content = notification.Content,
                NotificationType = notification.NotificationType,
                ReferenceId = notification.ReferenceId,
                ReferenceType = notification.ReferenceType,
                Link = notification.Link,
                CreatedAt = notification.CreatedAt,
                IsRead = notiRecipient?.IsRead,
                ReadAt = notiRecipient?.ReadAt,
            };
        }

        public static List<NotificationDto> ToNotificationDtos(this IEnumerable<Notification> notifications, int? userId = null)
        {
            return notifications.Select(notification => notification.ToNotificationDto(userId)).ToList();
        }
    }
}
