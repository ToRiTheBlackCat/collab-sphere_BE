using CollabSphere.Application.DTOs.ChatConversations;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Domain.Entities;
using CollabSphere.Application.Mappings.ChatMessages;
using CollabSphere.Application.Mappings.ClassMembers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollabSphere.Application.DTOs.ClassMembers;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Mappings.Lecturer;

namespace CollabSphere.Application.DTOs.ChatConversations
{
    public class ChatConversationDetailDto
    {
        public int ConversationId { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public string ConversationName { get; set; }

        public ChatConversationMessageVM? LatestMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<ChatConversationMessageVM> ChatMessages { get; set; } = new List<ChatConversationMessageVM>();

        public LecturerVM? Lecturer { get; set; }

        public List<ClassMemberVM> TeamMembers { get; set; } = new List<ClassMemberVM>();
    }
}

namespace CollabSphere.Application.Mappings.ChatConversations
{
    public static partial class ChatConversationMappings
    { 
        public static ChatConversationDetailDto ToDetailDto(this ChatConversation chatConversation)
        {
            // Get latest message
            var latestMessage = chatConversation.ChatMessages.OrderByDescending(x => x.SendAt).FirstOrDefault();

            // Setup chat messages to show who has read the messages
            var readUserIds = new HashSet<int>();
            foreach (var chatMessage in chatConversation.ChatMessages.Reverse())
            {
                chatMessage.MessageRecipients = chatMessage.MessageRecipients
                    .Where(x => 
                        x.IsRead && 
                        !readUserIds.Contains(x.ReceiverId))
                    .ToList();

                readUserIds = readUserIds.Concat(chatMessage.MessageRecipients.Select(x => x.ReceiverId)).ToHashSet();
            }

            return new ChatConversationDetailDto()
            {
                ConversationId = chatConversation.ConversationId,
                ConversationName = chatConversation.ConversationName,
                TeamId = chatConversation.TeamId,
                TeamName = chatConversation.Team?.TeamName ?? "NOT FOUND",
                CreatedAt = chatConversation.CreatedAt,
                LatestMessage = latestMessage?.ToChatConversatiobMessageVM(),
                ChatMessages = chatConversation.ChatMessages.ToChatConversatiobMessageVMs(),
                Lecturer = chatConversation.Team?.Class.Lecturer.ToViewModel() ?? null,
                TeamMembers = chatConversation.Team?.ClassMembers.ToViewModel() ?? new List<ClassMemberVM>()
            };
        }
    }
}
