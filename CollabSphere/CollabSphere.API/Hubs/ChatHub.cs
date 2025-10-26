using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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

        // Helper functions
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



        public async Task JoinChatConversation(int conversationId)
        {
            var (userId, name, userRole) = await GetUserInfo();
            var chatConversation = await _unitOfWork.ChatConversationRepo.GetConversationDetail(conversationId);
            if (chatConversation == null)
            {
                throw new HubException($"No conversation with ID '{conversationId}'.");
            }
            var groupName = chatConversation.ConversationName;

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Notify others new user has joined
            await Clients.OthersInGroup(groupName).UserJoined(userId);

            var recentMessages = chatConversation.ChatMessages.Take(50);
            var historyDtos = recentMessages.ToChatMessageDto();

            // Load recent messages for caller
            await Clients.Caller.ReceiveHistory(historyDtos);
        }

        public async Task BroadCastMessage(int conversationId, string newMessage)
        {
            var userInfo = await GetUserInfo();
            var chatConversation = await _unitOfWork.ChatConversationRepo.GetById(conversationId);
            if (chatConversation == null)
            {
                throw new HubException($"No conversation with ID '{conversationId}'.");
            }
            var groupName = chatConversation.ConversationName;

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                Message = newMessage,
                SendAt = DateTime.UtcNow,
                SenderId = userInfo.UserId,
            };

            try
            {
                // Add to DB
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.ChatMessageRepo.Create(message);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Setup for mapping
                message.Sender = new User()
                {
                    UId = userInfo.UserId,
                    IsTeacher = userInfo.UserRole == RoleConstants.LECTURER ? true : false,
                    Lecturer = new Lecturer() { Fullname = userInfo.FullName },
                    Student = new Student() { Fullname = userInfo.FullName }
                };

                // Broadcast message for others
                await Clients.Group(groupName).ReceiveMessage(message.ToChatMessageDto());
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
