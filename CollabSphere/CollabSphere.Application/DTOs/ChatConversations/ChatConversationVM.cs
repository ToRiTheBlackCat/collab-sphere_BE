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

        public int TeamId { get; set; }

        public string TeamName { get; set; } = null!;

        public string ConversationName { get; set; } = null!;

        public ChatMessageDto? LatestMessage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.ChatConversations
{
    public static partial class ChatConversationMappings
    {
        public static ChatConversationVM ToViewModel(this ChatConversation conversation)
        {
            // Get latest message
            var latestMessage = conversation.ChatMessages.OrderByDescending(x => x.SendAt).FirstOrDefault();

            return new ChatConversationVM()
            {
                ConversationId = conversation.ConversationId,
                ConversationName = conversation.ConversationName,
                TeamId = conversation.TeamId,
                TeamName = conversation.Team.TeamName,
                CreatedAt = conversation.CreatedAt,
                LatestMessage = latestMessage?.ToChatMessageDto(),
            };
        }

        public static List<ChatConversationVM> ToViewModels(this IEnumerable<ChatConversation> conversations)
        {
            if (conversations == null || !conversations.Any())
            {
                return new List<ChatConversationVM>();
            }

            return conversations.Select(x => x.ToViewModel()).ToList();
        }
    }
}
