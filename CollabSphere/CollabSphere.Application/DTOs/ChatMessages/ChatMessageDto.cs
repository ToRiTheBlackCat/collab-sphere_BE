using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ChatMessages
{
    public class ChatMessageDto
    {
        public int MessageId { get; set; }

        public int ConversationId { get; set; }

        public int SenderId { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime SendAt { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.ChatMessages
{
    public static partial class ChatMessageMappings
    {
        public static ChatMessageDto ToChatMessageDto(this ChatMessage message)
        {
            var senderName = "NOT FOUND USER";
            if (message.Sender != null)
            {
                // Sender is Lecturer
                if (message.Sender.IsTeacher)
                {
                    senderName = message.Sender.Lecturer?.Fullname ?? senderName;
                }
                // Sender is Student
                else
                {
                    senderName = message.Sender.Student?.Fullname ?? senderName;
                }
            }

            return new ChatMessageDto()
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = senderName,
                Message = message.Message,
                SendAt = message.SendAt,
            };
        }

        public static List<ChatMessageDto> ToChatMessageDto(this IEnumerable<ChatMessage> messages)
        {
            return messages.Select(x => x.ToChatMessageDto()).ToList();
        }
    }
}