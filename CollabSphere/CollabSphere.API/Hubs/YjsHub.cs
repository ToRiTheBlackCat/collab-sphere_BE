using CollabSphere.Application;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        // Dictionary<roomName, List<byte[]>>
        // This stores a list of all historical updates (as binary data) for each room.
        //private static readonly ConcurrentDictionary<string, List<string>> DocumentStates = new();

        // Dictionary<connectionId, userId>
        private static readonly ConcurrentDictionary<string, int> UserConnectionIds = new();

        // Tracks which rooms a specific connection is in for proper disconnect notifications.
        private static readonly ConcurrentDictionary<string, HashSet<string>> ConnectionRooms = new();

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<YjsHub> _logger; // ADDED: For logging

        public YjsHub(IUnitOfWork unitOfWork, ILogger<YjsHub> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // When a client joins a "room"
        public async Task JoinDocument(string room)
        {
            try
            {
                var userIdClaim = Context.User!.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);

                // Track connection ID to User ID
                UserConnectionIds.TryAdd(Context.ConnectionId, userId);

                // ADDED: Track that this connection is in this room
                ConnectionRooms.GetOrAdd(Context.ConnectionId, _ => new HashSet<string>()).Add(room);

                await Groups.AddToGroupAsync(Context.ConnectionId, room);

                var docStates = await _unitOfWork.DocStateRepo.GetStatesByRoom(room);
                var latestUpdates = docStates.Select(x => x.UpdateData).ToArray();

                await Clients.Caller.ReceiveDocState(latestUpdates);

                // 3. Notify others of the new connection
                // ... (notify and return)
            }
            catch (Exception ex)
            {
                // FIX: Log the error instead of swallowing it.
                _logger.LogError(ex, "Error in JoinDocument for room {Room} by user {ConnectionId}", room, Context.ConnectionId);
                // Optionally notify the caller of the failure
                throw new HubException($"Failed to join document. Please try again. Message: {ex}");
            }
        }

        // Broadcast Yjs document updates
        public async Task BroadcastUpdate(string room, string update)
        {
            try
            {
                // CRITICAL - Persist the new update to the database
                var newDocState = new DocumentState 
                {
                    RoomId = room,
                    UpdateData = update,
                    CreatedTime = DateTime.UtcNow,
                };
                await _unitOfWork.DocStateRepo.Create(newDocState);
                await _unitOfWork.SaveChangesAsync();

                // Forward to others in the room
                await Clients.OthersInGroup(room).ReceiveUpdate(update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BroadcastUpdate for room {Room}", room);
            }
        }

        // Replace updates in DB with snapshot
        public async Task SendMergedSnapshot(string room, string snapshotBase64)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Delete previous document states
                var docStates = await _unitOfWork.DocStateRepo.GetStatesByRoom(room);
                foreach (var docState in docStates)
                {
                    _unitOfWork.DocStateRepo.Delete(docState);
                }
                await _unitOfWork.SaveChangesAsync();

                // Create new state with merged snapshot
                var newDocState = new DocumentState
                {
                    RoomId = room,
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
                _logger.LogError(ex, "Error in BroadcastUpdate for room {Room}", room);
            }
        }

        // Broadcast Yjs awareness updates (cursor positions, selections, etc.)
        public async Task BroadcastAwareness(string room, string updateBase64)
        {
            try
            {
                await Clients.OthersInGroup(room).ReceiveAwareness(updateBase64);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error in BroadcastAwareness for room {Room}", room);
            }
        }

        // User LeaveDocument
        public async Task LeaveDocument(string room)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

            // Remove this room from the connection's tracking
            if (ConnectionRooms.TryGetValue(Context.ConnectionId, out var rooms))
            {
                rooms.Remove(room);
            }

            // Notify the *specific room* that the user left
            if (UserConnectionIds.TryGetValue(Context.ConnectionId, out var userId))
            {
                await Clients.Group(room).UserDisconnected(userId);
            }

            // DO NOT call OnDisconnectedAsync manually.
        }

        // Optional: return a snapshot if requested
        //public async Task<string?> RequestSnapshot(string room, string stateVectorBase64)
        //{
        //    // TODO: you can store snapshots in Redis or memory
        //    // For now return null (no snapshot stored)
        //    return null;
        //}

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Try to get the user ID for the disconnecting client
            if (UserConnectionIds.TryRemove(Context.ConnectionId, out var userId))
            {
                // Try to get the list of rooms the user was in
                if (ConnectionRooms.TryRemove(Context.ConnectionId, out var rooms))
                {
                    // Create a task to notify each room
                    var notificationTasks = rooms.Select(room =>
                        Clients.Group(room).UserDisconnected(userId)
                    );
                    await Task.WhenAll(notificationTasks);
                }

                _logger.LogInformation("User {UserId} (Connection {ConnectionId}) disconnected.", userId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
