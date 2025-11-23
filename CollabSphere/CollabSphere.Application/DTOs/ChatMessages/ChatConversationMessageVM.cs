using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ChatMessages
{
    public class ChatConversationMessageVM
    {
        public int MessageId { get; set; }

        public int ConversationId { get; set; }

        public int SenderId { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime SendAt { get; set; }

        /// <summary>
        /// UID of users who have read the message
        /// </summary>
        public List<int> ReadUserIds { get; set; } = new List<int>();
    }
}

namespace CollabSphere.Application.Mappings.ChatMessages
{
    public static partial class ChatMessageMappings
    {
        public static ChatConversationMessageVM ToChatConversatiobMessageVM(this ChatMessage chatMessage)
        {
            var senderName = "NOT FOUND USER";
            if (chatMessage.Sender != null)
            {
                // Sender is Lecturer
                if (chatMessage.Sender.IsTeacher)
                {
                    senderName = chatMessage.Sender.Lecturer?.Fullname ?? senderName;
                }
                // Sender is Student
                else
                {
                    senderName = chatMessage.Sender.Student?.Fullname ?? senderName;
                }
            }

            // Get UIDs of users who have read the message
            var readUserIds = new List<int>();
            if (chatMessage.MessageRecipients != null)
            {
                readUserIds = chatMessage.MessageRecipients
                    .Where(recipient => recipient.IsRead)
                    .Select(x => x.ReceiverId)
                    .ToList();
            }

            return new ChatConversationMessageVM()
            {
                MessageId = chatMessage.MessageId,
                ConversationId = chatMessage.ConversationId,
                Message =  chatMessage.Message,
                SenderId = chatMessage.SenderId,
                SenderName = senderName,
                SendAt = chatMessage.SendAt,
                ReadUserIds = readUserIds,
            };
        }

        public static List<ChatConversationMessageVM> ToChatConversatiobMessageVMs(this IEnumerable<ChatMessage> chatMessages)
        {
            if (chatMessages == null || !chatMessages.Any())
            {
                return new List<ChatConversationMessageVM>();
            }

            return chatMessages.Select(x => x.ToChatConversatiobMessageVM()).ToList();
        }
    }
}