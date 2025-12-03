using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ChatConversations;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Application.Mappings.ChatMessages;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ChatConversations
{
    public class ChatConversationVM
    {
        public int ConversationId { get; set; }

        public string ConversationName { get; set; } = null!;

        public string Type
        {
            get
            {
                var type = this.TeamId.HasValue ? ConversationTypes.TEAM_CONVERSTAION : ConversationTypes.CLASS_CONVESATION;
                return type.ToString();
            }
        }

        public int ClassId { get; set; }

        public string ClassName { get; set; } = null!;

        public int? TeamId { get; set; }

        public string TeamName { get; set; } = null!;

        public int SemesterId { get; set; }

        public string SemesterName { get; set; } = null!;

        public string SemesterCode { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public int MessageCount { get; set; }

        public bool IsRead { get; set; }

        public int UnreadCount { get; set; }

        public ChatConversationMessageVM? LatestMessage { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.ChatConversations
{
    public static partial class ChatConversationMappings
    {
        public static ChatConversationVM ToViewModel(this ChatConversation conversation, int? viewingUserId = null)
        {
            // Get latest message
            var latestMessage = conversation.ChatMessages.OrderByDescending(x => x.SendAt).FirstOrDefault();
            var hasRead = true;
            var unreadCount = 0;
            if (latestMessage != null && viewingUserId.HasValue)
            {
                latestMessage.MessageRecipients.Add(new MessageRecipient()
                {
                    ReceiverId = latestMessage.SenderId,
                    IsRead = true,
                });

                unreadCount = conversation.ChatMessages
                    .SelectMany(x => x.MessageRecipients)
                    .Where(x => 
                        x.ReceiverId == viewingUserId && 
                        !x.IsRead)
                    .Count();
                hasRead = unreadCount == 0;
            }

            var teamName = "";
            if (conversation.TeamId.HasValue)
            {
                teamName = conversation.Team?.TeamName ?? "NOT FOUND";
            }

            return new ChatConversationVM()
            {
                ConversationId = conversation.ConversationId,
                ConversationName = conversation.ConversationName,
                ClassId = conversation.ClassId,
                ClassName = conversation.Class?.ClassName ?? "NOT FOUND",
                SemesterId = conversation.Class?.SemesterId ?? -1,
                SemesterName = conversation.Class?.Semester?.SemesterName ?? "NOT_FOUND",
                SemesterCode = conversation.Class?.Semester?.SemesterCode ?? "NOT_FOUND",
                TeamId = conversation.TeamId,
                TeamName = teamName,
                IsRead = hasRead,
                UnreadCount = unreadCount,
                CreatedAt = conversation.CreatedAt,
                MessageCount = conversation.ChatMessages.Count,
                LatestMessage = latestMessage?.ToChatConversatiobMessageVM(),
            };
        }

        public static List<ChatConversationVM> ToViewModels(this IEnumerable<ChatConversation> conversations, int? viewingUserId = null)
        {
            if (conversations == null || !conversations.Any())
            {
                return new List<ChatConversationVM>();
            }

            return conversations.Select(x => x.ToViewModel(viewingUserId)).ToList();
        }
    }
}
