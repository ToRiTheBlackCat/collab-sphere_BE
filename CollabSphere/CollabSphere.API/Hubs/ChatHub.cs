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

    [Authorize(Roles = "4, 5")]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Mappings of Connection IDs for a UserId
        private static readonly ConcurrentDictionary<int, List<string>> UserConnectionIds = new();

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

        private (bool canJoin, string groupName, string errorMessage) CanJoinConversation(int userId, int userRole, ChatConversation conversation)
        {
            var canJoin = true;
            var errorMessage = string.Empty;
            var groupName = $"conv-{conversation.ConversationId}-{conversation.ConversationName}";
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
                var isValidLecturer = conversation.Team.Class.LecturerId == userId;
                if (!isValidLecturer)
                {
                    canJoin = false;
                    errorMessage = $"You ({userId}) are the assigned lecturer of the class with ID '{conversation.Team.Class.ClassId}'.";
                }
            }

            return (canJoin: canJoin, groupName: groupName, errorMessage: errorMessage);
        }

        public async Task JoinChatConversation(int conversationId)
        {
            var (userId, name, userRole) = await GetUserInfo();
            var chatConversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(conversationId);
            if (chatConversation == null)
            {
                throw new HubException($"No conversation with ID '{conversationId}'.");
            }

            // Check if can join conversation
            var joinResult = this.CanJoinConversation(userId, userRole, chatConversation);
            if (!joinResult.canJoin)
            {
                throw new HubException($"Can not join conversation. Reason: {joinResult.errorMessage}");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, joinResult.groupName);

            // Save connection ID
            var connectionsOfUser = UserConnectionIds.GetOrAdd(userId, _ => new List<string>() { Context.ConnectionId });

            // Notify others new user has joined
            await Clients.OthersInGroup(joinResult.groupName).UserJoined(userId);

            // Load recent messages for caller
            var recentMessages = chatConversation.ChatMessages;
            var historyDtos = recentMessages.ToChatMessageDto();

            await Clients.Caller.ReceiveHistory(historyDtos);

            // Load recent notifications for caller
            var recentNotifications = await _unitOfWork.NotificationRepo.GetChatNotificationsOfUser(userId);
            var notiDtos = recentNotifications.ToChatNotiDto().Take(50);

            await Clients.Caller.ReceiveAllNotification(notiDtos);
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
            var joinResult = this.CanJoinConversation(userInfo.UserId, userInfo.UserRole, chatConversation);
            if (!joinResult.canJoin)
            {
                throw new HubException($"Can not join conversation. Reason: {joinResult.errorMessage}");
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
                    SendAt = currentTime,
                    SenderId = userInfo.UserId,
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

                // Create notifications for other students in team
                foreach (var teamMember in chatConversation.Team.ClassMembers)
                {
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

                // Broadcast message for others
                await Clients.Group(joinResult.groupName).ReceiveMessage(message.ToChatMessageDto());

                // Also broadcast notification
                var notiTasks = new List<Task>();
                foreach (var teamMember in chatConversation.Team.ClassMembers)
                {
                    if (UserConnectionIds.TryGetValue(teamMember.StudentId, out var connectionIds))
                    {
                        var memberNotiTasks = connectionIds
                            .Select(connection => Clients.Client(connection)
                            .ReceiveNotification(notification.ToChatNotiDto()));
                        notiTasks.AddRange(memberNotiTasks);
                    }
                }
                await Task.WhenAll(notiTasks);
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

            if (UserConnectionIds.TryRemove(userId, out var connectionIds))
            {
                _ = Clients.All.UserLeft(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
