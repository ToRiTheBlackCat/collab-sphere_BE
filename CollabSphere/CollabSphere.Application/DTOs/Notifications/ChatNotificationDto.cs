using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Notifications
{
    public class ChatNotificationDto
    {
        public int NotificationId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string NotificationType { get; set; }

        public int? ReferenceId { get; set; }

        public string ReferenceType { get; set; }

        public string Link { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public static partial class NotificationMappings
    {
        public static ChatNotificationDto ToChatNotiDto(this Notification notification)
        {
            return new ChatNotificationDto()
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Content = notification.Content,
                NotificationType = notification.NotificationType,
                ReferenceId = notification.ReferenceId,
                ReferenceType = notification.ReferenceType,
                Link = notification.Link,
                CreatedAt = notification.CreatedAt,
            };
        }

        public static List<ChatNotificationDto> ToChatNotiDto(this IEnumerable<Notification> notifications)
        {
            return notifications.Select(notification => notification.ToChatNotiDto()).ToList();
        }
    }
}
