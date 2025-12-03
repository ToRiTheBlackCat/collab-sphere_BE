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
using static CollabSphere.Application.DTOs.ChatConversations.ChatConversationDetailDto;
using CollabSphere.Application.Constants;

namespace CollabSphere.Application.DTOs.ChatConversations
{
    public partial class ChatConversationDetailDto
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

        public ChatUserDto? Lecturer { get; set; }

        public List<ChatUserDto> TeamMembers { get; set; } = new List<ChatUserDto>();

        public ChatConversationMessageVM? LatestMessage { get; set; }

        public List<ChatConversationMessageVM> ChatMessages { get; set; } = new List<ChatConversationMessageVM>();
    }

    public partial class ChatConversationDetailDto
    {
        public class ChatUserDto
        {
            public int UserId { get; set; }

            public string FullName { get; set; } = null!;

            public string AvatarImg { get; set; } = null!;

            public bool IsTeacher { get; set; }
        }
    }
}

namespace CollabSphere.Application.Mappings.ChatConversations
{
    public static partial class ChatConversationMappings
    {
        public static ChatUserDto ToChatUserDto(this CollabSphere.Domain.Entities.User user)
        {
            var avatarImg = "NOT_FOUND";
            var fullName = "NOT_FOUND";
            if (user.IsTeacher)
            {
                avatarImg = user.Lecturer.AvatarImg;
                fullName = user.Lecturer.Fullname;
            }
            else
            {
                avatarImg = user.Student.AvatarImg;
                fullName = user.Student.Fullname;
            }

            return new ChatUserDto()
            {
                UserId = user.UId,
                IsTeacher = user.IsTeacher,
                AvatarImg = avatarImg,
                FullName = fullName,
            };
        }

        public static List<ChatUserDto> ToChatUserDtos(this IEnumerable<CollabSphere.Domain.Entities.User> users)
        {
            if (users == null || !users.Any())
            {
                return new List<ChatUserDto>();
            }

            return users.Select(x => x.ToChatUserDto()).ToList();
        }

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

                // Add a fake recipient so the User who sent the message is also counted as a user who has read it
                if (!readUserIds.Contains(chatMessage.SenderId))
                {
                    chatMessage.MessageRecipients.Add(new MessageRecipient()
                    {
                        ReceiverId = chatMessage.SenderId,
                        IsRead = true,
                    });
                }

                readUserIds = readUserIds.Concat(chatMessage.MessageRecipients.Select(x => x.ReceiverId)).ToHashSet();
            }

            var teamName = "";
            if (chatConversation.TeamId.HasValue)
            {
                teamName = chatConversation.Team?.TeamName ?? "NOT FOUND";
            }

            return new ChatConversationDetailDto()
            {
                ConversationId = chatConversation.ConversationId,
                ConversationName = chatConversation.ConversationName,
                ClassId = chatConversation.ClassId,
                ClassName = chatConversation.Class?.ClassName ?? "NOT FOUND",
                TeamId = chatConversation.TeamId,
                TeamName = teamName,
                SemesterId = chatConversation.Class?.SemesterId ?? -1,
                SemesterName = chatConversation.Class?.Semester?.SemesterName ?? "NOT_FOUND",
                SemesterCode = chatConversation.Class?.Semester?.SemesterCode ?? "NOT_FOUND",
                CreatedAt = chatConversation.CreatedAt,
                Lecturer = chatConversation.Users.SingleOrDefault(x => x.IsTeacher)?.ToChatUserDto(),
                TeamMembers = chatConversation.Users.Where(x => !x.IsTeacher).ToChatUserDtos() ?? new List<ChatUserDto>(),
                LatestMessage = latestMessage?.ToChatConversatiobMessageVM(),
                ChatMessages = chatConversation.ChatMessages.ToChatConversatiobMessageVMs(),
            };
        }
    }
}
