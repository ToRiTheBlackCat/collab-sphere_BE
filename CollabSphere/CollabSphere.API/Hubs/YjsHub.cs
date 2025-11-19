using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace CollabSphere.API.Hubs
{
    public interface IDocumentClient
    {
        Task ReceiveUpdate(string updateBase64);

        Task ReceiveAwareness(string updateBase64);

        Task ReceiveDocState(string[] updateBase64s);

        Task UserDisconnected(int userId);
    }

    [Authorize]
    public class YjsHub : Hub<IDocumentClient>
    {
        // Dictionary<connectionId, userId>
        // Track UserId with ConnectionId for quick access
        private static readonly ConcurrentDictionary<string, int> UserConnectionIds = new();

        // Dictionary<connectionId, RoomId>
        // Tracks which rooms a specific connection is in for proper disconnect notifications.
        private static readonly ConcurrentDictionary<string, HashSet<string>> ConnectedRooms = new();

        // Dictionary<room, connectionIds>
        // Track Valid ConnectionIds to room for quick validation
        private static readonly ConcurrentDictionary<string, HashSet<string>> RoomConnections = new();

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<YjsHub> _logger; // ADDED: For logging

        public YjsHub(IUnitOfWork unitOfWork, ILogger<YjsHub> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

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

        // When a client joins a "room"
        public async Task JoinDocument(int teamId, string roomName)
        {
            var groupString = $"{teamId}_{roomName}";

            try
            {
                var userInfo = await GetUserInfo();

                // Get document room and states
                var docRoom = await _unitOfWork.DocRoomRepo.GetDocumentRoomDetail(teamId, roomName);
                if (docRoom == null)
                {
                    throw new Exception($"The team with ID '{teamId}' does not have a document room of '{roomName}'.");
                }
                else
                {
                    // Check if is lecturer of the class
                    if (userInfo.UserRole == RoleConstants.LECTURER && userInfo.UserId != docRoom.Team.Class.LecturerId)
                    {
                        throw new Exception($"You ({teamId}) are not the assigned lecturer of the team with ID '{teamId}'.");
                    }
                    // Check if is member in team
                    else if (userInfo.UserRole == RoleConstants.STUDENT)
                    {
                        var isMember = docRoom.Team.ClassMembers.Any(x => x.StudentId == userInfo.UserId);
                        if (!isMember)
                        {
                            throw new Exception($"You ({teamId}) are not a member of the team with ID '{teamId}'.");
                        }
                    }
                }

                var latestUpdates = docRoom.DocumentStates.Select(x => x.UpdateData).ToArray();

                // Bind User ID to Connection
                UserConnectionIds.TryAdd(Context.ConnectionId, userInfo.UserId);

                // Bind Room to this specific ConnectionId for latter disconnection broadcast
                ConnectedRooms.GetOrAdd(Context.ConnectionId, _ => new HashSet<string>()).Add(groupString);

                // Bind ConnectionId to room for faster validation
                RoomConnections.GetOrAdd(groupString, _ => new HashSet<string>()).Add(Context.ConnectionId);

                await Groups.AddToGroupAsync(Context.ConnectionId, groupString);

                await Clients.Caller.ReceiveDocState(latestUpdates);
            }
            catch (Exception ex)
            {
                // Log the error instead of swallowing it.
                _logger.LogError(ex, $"Error in JoinDocument for room {roomName} by user {Context.ConnectionId}", roomName, Context.ConnectionId);
                // Notifiy the caller of the failure
                throw new HubException($"Failed to join document. Please try again. Message: {ex}");
            }
        }

        // Broadcast Yjs document updates
        public async Task BroadcastUpdate(int teamId, string roomName, string update)
        {
            var groupString = $"{teamId}_{roomName}";

            try
            {
                // Validate user
                if (RoomConnections.TryGetValue(groupString, out var connectionIds))
                {
                    if(!connectionIds.Contains(Context.ConnectionId))
                    {
                        var userId = UserConnectionIds.TryGetValue(Context.ConnectionId, out var foundUserId) ? foundUserId : -1;
                        throw new HubException($"You ({userId}) are not allow to edit this room.");
                    }
                }

                // Persist the new update to the database
                var newDocState = new DocumentState
                {
                    TeamId = teamId,
                    RoomName = roomName,
                    UpdateData = update,
                    CreatedTime = DateTime.UtcNow,
                };
                await _unitOfWork.DocStateRepo.Create(newDocState);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BroadcastUpdate for room {Room}", roomName);
                throw new HubException($"Failed to broadcast update. Message: {ex}");
            }

            // Forward to others in the room
            var broadcastUpdate = Clients.OthersInGroup(groupString).ReceiveUpdate(update);
        }

        // Replace updates in DB with snapshot
        public async Task SendMergedSnapshot(int teamId, string roomName, string snapshotBase64)
        {
            if (string.IsNullOrWhiteSpace(snapshotBase64))
            {
                return;
            }

            var groupString = $"{teamId}_{roomName}";

            try
            {
                // Validate user
                if (RoomConnections.TryGetValue(groupString, out var connectionIds))
                {
                    if (!connectionIds.Contains(Context.ConnectionId))
                    {
                        var userId = UserConnectionIds.TryGetValue(Context.ConnectionId, out var foundUserId) ? foundUserId : -1;
                        throw new HubException($"You ({userId}) are not allow to edit this room.");
                    }
                }

                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Delete previous document states
                var docStates = await _unitOfWork.DocStateRepo.GetStatesByDocumentRoom(teamId, roomName);
                foreach (var docState in docStates)
                {
                    _unitOfWork.DocStateRepo.Delete(docState);
                }
                await _unitOfWork.SaveChangesAsync();

                // Create new state with merged snapshot
                var newDocState = new DocumentState
                {
                    RoomName = roomName,
                    UpdateData = snapshotBase64,
                    CreatedTime = DateTime.UtcNow,
                };
                await _unitOfWork.DocStateRepo.Create(newDocState);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error in BroadcastUpdate for room {Room}", roomName);
            }
        }

        // Broadcast Yjs awareness updates (cursor positions, selections, etc.)
        public void BroadcastAwareness(int teamId, string roomName, string updateBase64)
        {
            var groupString = $"{teamId}_{roomName}";

            try
            {

                // Validate user
                if (RoomConnections.TryGetValue(groupString, out var connectionIds))
                {
                    if (!connectionIds.Contains(Context.ConnectionId))
                    {
                        var userId = UserConnectionIds.TryGetValue(Context.ConnectionId, out var foundUserId) ? foundUserId : -1;
                        throw new HubException($"You ({userId}) are not allow to edit this room.");
                    }
                }

                Clients.OthersInGroup(groupString).ReceiveAwareness(updateBase64);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error in BroadcastAwareness for room {Room}", roomName);
            }
        }

        // User LeaveDocument
        public async Task LeaveDocument(int teamId, string roomName)
        {
            var groupString = $"{teamId}_{roomName}";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupString);

            // Remove this ConnectionId from Room's valid connectionIds
            if (RoomConnections.TryGetValue(groupString, out var connections))
            {
                connections.Remove(Context.ConnectionId);
            }

            // Notify the *specific room* that the user left
            if (UserConnectionIds.TryRemove(Context.ConnectionId, out var disconnectUserId))
            {
                await Clients.OthersInGroup(groupString).UserDisconnected(disconnectUserId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Try to get the user ID for the disconnecting client
            if (UserConnectionIds.TryRemove(Context.ConnectionId, out var userId))
            {

            }

            // Try to get the list of rooms the user was in
            if (ConnectedRooms.TryRemove(Context.ConnectionId, out var roomGroups))
            {
                foreach (var roomGroup in roomGroups)
                {
                    // Remove from Room's valid connection IDs
                    if (RoomConnections.TryGetValue(roomGroup, out var roomConnectionIds))
                    {
                        roomConnectionIds.Remove(roomGroup);
                    }
                    
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomGroup);
                }

                // Create a task to notify each room of user disconnection
                var notificationTasks = roomGroups.Select(room =>
                    Clients.OthersInGroup(room).UserDisconnected(userId)
                );
                await Task.WhenAll(notificationTasks);
            }

            _logger.LogInformation("User {UserId} (Connection {ConnectionId}) disconnected.", userId, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
