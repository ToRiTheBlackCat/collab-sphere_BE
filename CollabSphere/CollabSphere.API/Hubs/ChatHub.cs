using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ChatConversations;
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
        Task ReceiveMessage(ChatConversationMessageVM message);

        // Corresponds to connection.on("ReceiveHistory", ...)
        Task ReceiveHistory(IEnumerable<ChatConversationMessageVM> messages);

        Task ReceiveNotification(NotificationDto notification);

        Task ReceiveAllNotification(IEnumerable<NotificationDto> notification);

        Task ReceiveMessageReadUpdate(int userId, int conversationId, int messageId);

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

        public ChatHub(IUnitOfWork unitOfWork, ConnectionMappings connectionMappings)
        {
            _unitOfWork = unitOfWork;
            _mapping = connectionMappings.ChatHubMapping;
        }

        /// <summary>
        /// Details of connectionIds
        /// Dictionary<ConnectionId, ConnectionInfo>
        /// </summary
        private readonly ConcurrentDictionary<string, ChatHubConnectionInfo> _mapping;

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

            var isInConversation = conversation.Users.Any(x => x.UId == userId);
            if (!isInConversation)
            {
                var conversationUsersStrings = conversation.Users.Select(x =>
                {
                    var fullName = x.IsTeacher ? x.Lecturer.Fullname : x.Student.Fullname;
                    var roleString = x.IsTeacher ? " (Lecturer)" : string.Empty;
                    return $"{fullName}({x.UId}){roleString}";
                });
                errorMessage = $"You({userId}) are not a user in this conversation. Valid users are: {string.Join(", ", conversationUsersStrings)}";
                canJoin = false;
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
                    var chatHubConnectionInfo = _mapping.GetOrAdd(Context.ConnectionId,
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
            _mapping.TryGetValue(Context.ConnectionId, out var connectionInfo);
            if (!connectionInfo!.ConversationIds.Contains(chatConversation.ConversationId))
            {
                throw new HubException($"Can not join conversation. This connection is not valid: {connectionInfo}");
            }
            else
            {
                // Load messages for caller
                var recentMessages = chatConversation.ChatMessages;
                var historyDtos = recentMessages.ToChatConversatiobMessageVMs();
                await Clients.Caller.ReceiveHistory(historyDtos);

                // Load recent notifications for caller
                var recentNotifications = await _unitOfWork.NotificationRepo.GetChatNotificationsOfUser(userInfo.UserId);
                var notiDtos = recentNotifications.ToNotificationDtos().Take(50);
                await Clients.Caller.ReceiveAllNotification(notiDtos);
            }
        }

        public async Task BroadCastMessage(int conversationId, string newMessage)
        {
            var userInfo = await GetUserInfo();

            // Check if can join conversation
            _mapping.TryGetValue(Context.ConnectionId, out var connectionInfo);
            if (!connectionInfo!.ConversationIds.Contains(conversationId))
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

                // Create message receipient read state for other user in conversastion
                var conversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(conversationId);
                foreach (var user in conversation!.Users)
                {
                    if (user.UId == userInfo.UserId)
                    {
                        continue;
                    }

                    var messageRecipient = new MessageRecipient()
                    {
                        MessageId = message.MessageId,
                        ReceiverId = user.UId,
                        IsRead = false,
                    };
                    await _unitOfWork.MessageRecipientRepo.Create(messageRecipient);
                }

                #region Update conversation latest message
                var updateConversation = await _unitOfWork.ChatConversationRepo.GetById(conversationId);
                updateConversation!.LatestMessageId = message.MessageId;

                _unitOfWork.ChatConversationRepo.Update(updateConversation);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                #region Realtime Broadcast Message
                // Setup for DTO mapping
                message.Sender = new User()
                {
                    UId = userInfo.UserId,
                    IsTeacher = userInfo.UserRole == RoleConstants.LECTURER ? true : false,
                    Lecturer = new Lecturer() { Fullname = userInfo.FullName },
                    Student = new Student() { Fullname = userInfo.FullName }
                };
                var messageDto = message.ToChatConversatiobMessageVM();

                // Broadcast to group
                await Clients.Groups($"{conversationId}").ReceiveMessage(messageDto); 
                #endregion
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException(ex.Message);
            }
        }

        public async Task BroadcastReadUpdate(int conversationId, int readMessageId)
        {
            var userInfo = await this.GetUserInfo();

            // Check if can join conversation
            if (_mapping.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                if (!connectionInfo.ConversationIds.Contains(conversationId))
                {
                    throw new HubException($"Can not join conversation. This connection is not valid: {connectionInfo}");
                }

                // Broadcast message read update to others
                await Clients.Group($"{conversationId}").ReceiveMessageReadUpdate(userInfo.UserId, conversationId, readMessageId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var (userId, name, userRole) = await GetUserInfo();

            if (_mapping.TryRemove(Context.ConnectionId, out var removedInfo))
            {
                // Remove connection Id from groups
                foreach (var conversationId in removedInfo.ConversationIds)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{conversationId}");
                }

                Console.WriteLine($"REMOVE USER_ID '{removedInfo.UserId}', CONNECTION_ID: {removedInfo.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
