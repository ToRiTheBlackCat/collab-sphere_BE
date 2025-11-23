using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Application.DTOs.Notifications;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.Mappings.ChatMessages;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.API.Hubs
{
    public interface IChatClient
    {
        // Corresponds to connection.on("ReceiveMessage", ...)
        Task ReceiveMessage(ChatMessageDto message);

        // Corresponds to connection.on("ReceiveHistory", ...)
        Task ReceiveHistory(IEnumerable<ChatMessageDto> messages);

        Task ReceiveNotification(ChatNotificationDto notification);

        Task ReceiveAllNotification(IEnumerable<ChatNotificationDto> notification);

        // Corresponds to connection.on("UserJoined", ...)
        Task UserJoined(int userId);

        // Corresponds to connection.on("UserLeft", ...)
        Task UserLeft(int userId);
    }

    public class ChatHubConnectionInfo
    {
        public required string ConnectionId { get; set; }

        public required int UserId { get; set; }

        public required List<int> ConversationIds { get; set; }
    }

    [Authorize(Roles = "4, 5")]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Details of connectionIds
        /// Dictionary<ConnectionId, ConnectionInfo>
        /// </summary
        private static readonly ConcurrentDictionary<string, ChatHubConnectionInfo> ConnectionInfos = new();

        // Helper function
        private async Task<(int UserId, string FullName, int UserRole)> GetUserInfo()
        {
            // Get UserId & Role of requester
            var userIdClaim = Context.User!.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = Context.User.Claims.First(c => c.Type == ClaimTypes.Role);
            var userId = int.Parse(userIdClaim.Value);
            var role = int.Parse(roleClaim.Value);

            if (role == RoleConstants.STUDENT)
            {
                var student = await _unitOfWork.StudentRepo.GetById(userId);
                return (userId, student!.Fullname, role);
            }
            else
            {
                var lecturer = await _unitOfWork.LecturerRepo.GetById(userId);
                return (userId, lecturer!.Fullname, role);
            }
        }

        private (bool canJoin, string errorMessage) CanJoinConversation(int userId, int userRole, ChatConversation conversation)
        {
            var canJoin = true;
            var errorMessage = string.Empty;

            if (userRole == RoleConstants.STUDENT)
            {
                // Check if a member of team
                var isTeamMember = conversation.Team
                    .ClassMembers
                    .Any(x => x.StudentId == userId);
                if (!isTeamMember)
                {
                    canJoin = false;
                    errorMessage = $"You ({userId}) are not a member of the team with ID '{conversation.Team.TeamId}'.";
                }
            }
            else if (userRole == RoleConstants.LECTURER)
            {
                // Check if is assigned lecturer
                var isValidLecturer = conversation.Team.Class.LecturerId == userId;
                if (!isValidLecturer)
                {
                    canJoin = false;
                    errorMessage = $"You ({userId}) are not the assigned lecturer of the class with ID '{conversation.Team.Class.ClassId}'.";
                }
            }

            return (canJoin: canJoin, errorMessage: errorMessage);
        }

        public async Task JoinChatConversation(List<int> conversationIds)
        {
            if (!conversationIds.Any())
            {
                throw new HubException($"No conversation IDs provided to join.");
            }

            var userInfo = await GetUserInfo();

            foreach (var conversationId in conversationIds)
            {
                var chatConversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(conversationId);
                if (chatConversation == null)
                {
                    throw new HubException($"No conversation with ID '{conversationId}'.");
                }

                // Check if can join conversation
                var joinResult = this.CanJoinConversation(userInfo.UserId, userInfo.UserRole, chatConversation);
                if (!joinResult.canJoin)
                {
                    throw new HubException($"Can not join conversation. Reason: {joinResult.errorMessage}");
                }
                else
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{conversationId}");

                    // Add this ConnectionId to ConnectionInfos
                    var chatHubConnectionInfo = ConnectionInfos.GetOrAdd(Context.ConnectionId,
                        new ChatHubConnectionInfo()
                        {
                            ConnectionId = Context.ConnectionId,
                            ConversationIds = new List<int> { conversationId },
                            UserId = userInfo.UserId,
                        });
                    chatHubConnectionInfo.ConversationIds.Add(conversationId);
                }
            }
        }

        public async Task LoadChatConversationHistory(int conversationId)
        {
            // Get user info from access token
            var userInfo = await GetUserInfo();

            var chatConversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(conversationId);
            if (chatConversation == null)
            {
                throw new HubException($"No conversation with ID '{conversationId}'.");
            }

            // Check if can join conversation
            ConnectionInfos.TryGetValue(Context.ConnectionId, out var connectionInfo);
            if (!connectionInfo!.ConversationIds.Contains(chatConversation.ConversationId))
            {
                throw new HubException($"Can not join conversation. This connection is not valid: {connectionInfo}");
            }
            else
            {
                // Load messages for caller
                var recentMessages = chatConversation.ChatMessages;
                var historyDtos = recentMessages.ToChatMessageDto();
                await Clients.Caller.ReceiveHistory(historyDtos);

                // Load recent notifications for caller
                var recentNotifications = await _unitOfWork.NotificationRepo.GetChatNotificationsOfUser(userInfo.UserId);
                var notiDtos = recentNotifications.ToChatNotiDto().Take(50);
                await Clients.Caller.ReceiveAllNotification(notiDtos);
            }
        }

        public async Task BroadCastMessage(int conversationId, string newMessage)
        {
            var userInfo = await GetUserInfo();
            var chatConversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(conversationId);
            if (chatConversation == null)
            {
                throw new HubException($"No conversation with ID '{conversationId}'.");
            }

            // Check if can join conversation
            ConnectionInfos.TryGetValue(Context.ConnectionId, out var connectionInfo);
            //if (!IsValidConnectionId(conversationId, Context.ConnectionId, out var errorString))
            if (!connectionInfo!.ConversationIds.Contains(chatConversation.ConversationId))
            {
                throw new HubException($"Can not join conversation. This connection is not valid: {connectionInfo}");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var currentTime = DateTime.UtcNow;

                // Add message entry
                var message = new ChatMessage
                {
                    ConversationId = conversationId,
                    Message = newMessage,
                    SenderId = userInfo.UserId,
                    SendAt = currentTime,
                };
                await _unitOfWork.ChatMessageRepo.Create(message);
                await _unitOfWork.SaveChangesAsync();

                // Create notifications
                var notification = new Notification()
                {
                    Title = "New Converstaion Message",
                    Content = $"New message in '{chatConversation.ConversationName}' from: {userInfo.FullName}",
                    NotificationType = NotificationTypes.ChatNotification.ToString(),
                    ReferenceId = -1,
                    ReferenceType = "",
                    Link = "",
                    CreatedAt = currentTime,
                };
                await _unitOfWork.NotificationRepo.Create(notification);
                await _unitOfWork.SaveChangesAsync();

                // Create notification & Message for lecturer (If the sender is not the lecturer themself)
                if (chatConversation.Team.Class.LecturerId.HasValue && chatConversation.Team.Class.LecturerId != userInfo.UserId)
                {
                    var messageRecipient = new MessageRecipient()
                    {
                        MessageId = message.MessageId,
                        ReceiverId = chatConversation.Team.Class.LecturerId.Value,
                        IsRead = false,
                    };
                    await _unitOfWork.MessageRecipientRepo.Create(messageRecipient);

                    var notiRecipient = new NotificationRecipient()
                    {
                        ReceiverId = chatConversation.Team.Class.LecturerId.Value,
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(notiRecipient);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Create notification & Message for other students in team
                foreach (var teamMember in chatConversation.Team.ClassMembers)
                {
                    // Skip sender
                    if (teamMember.StudentId == userInfo.UserId)
                    {
                        continue;
                    }

                    var messageRecipient = new MessageRecipient()
                    {
                        MessageId = message.MessageId,
                        ReceiverId = teamMember.StudentId,
                        IsRead = false,
                    };
                    await _unitOfWork.MessageRecipientRepo.Create(messageRecipient);

                    var notiRecipient = new NotificationRecipient()
                    {
                        ReceiverId = teamMember.StudentId,
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(notiRecipient);
                }
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Setup for DTO mapping
                message.Sender = new User()
                {
                    UId = userInfo.UserId,
                    IsTeacher = userInfo.UserRole == RoleConstants.LECTURER ? true : false,
                    Lecturer = new Lecturer() { Fullname = userInfo.FullName },
                    Student = new Student() { Fullname = userInfo.FullName }
                };
                var messageDto = message.ToChatMessageDto();

                // Broadcast to group
                await Clients.Groups($"{conversationId}").ReceiveMessage(messageDto);
                await Clients.OthersInGroup($"{conversationId}").ReceiveNotification(notification.ToChatNotiDto());
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException(ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var (userId, name, userRole) = await GetUserInfo();

            if (ConnectionInfos.TryRemove(Context.ConnectionId, out var removedInfo))
            {
                // Remove connection Id from groups
                foreach(var conversationId in removedInfo.ConversationIds)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{conversationId}");
                }

                Console.WriteLine($"REMOVE USER_ID '{removedInfo.UserId}', CONNECTION_ID: {removedInfo.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
