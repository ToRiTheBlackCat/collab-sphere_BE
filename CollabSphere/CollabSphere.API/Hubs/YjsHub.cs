using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

        // Dictionary<room, ConnectionIds>
        private static readonly ConcurrentDictionary<string, HashSet<string>> RoomConnections = new();

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

                // Add connection to Room's connections list for tracking
                RoomConnections.GetOrAdd(room, _ => new HashSet<string>()).Add(Context.ConnectionId);

                await Groups.AddToGroupAsync(Context.ConnectionId, room);

                var docStates = await _unitOfWork.DocStateRepo.GetStatesByRoom(room);
                var latestUpdates = docStates.Select(x => x.UpdateData).ToArray();

                await Clients.Caller.ReceiveDocState(latestUpdates);
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
                // Persist the new update to the database
                var newDocState = new DocumentState 
                {
                    RoomId = room,
                    UpdateData = update,
                    CreatedTime = DateTime.UtcNow,
                };
                await _unitOfWork.DocStateRepo.Create(newDocState);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BroadcastUpdate for room {Room}", room);
            }

            // Forward to others in the room
            var broadcastUpdate = Clients.OthersInGroup(room).ReceiveUpdate(update);
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
        public void BroadcastAwareness(string room, string updateBase64)
        {
            try
            {
                Clients.OthersInGroup(room).ReceiveAwareness(updateBase64);
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
            if (RoomConnections.TryGetValue(room, out var connections))
            {
                connections.Remove(Context.ConnectionId);
            }

            // Notify the *specific room* that the user left
            if (UserConnectionIds.TryGetValue(Context.ConnectionId, out var userId))
            {
                await Clients.Group(room).UserDisconnected(userId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Try to get the user ID for the disconnecting client
            if (UserConnectionIds.TryRemove(Context.ConnectionId, out var userId))
            {
                // Try to get the list of rooms the user was in
                if (RoomConnections.TryRemove(Context.ConnectionId, out var rooms))
                {
                    //// Create a task to notify each room ()
                    //var notificationTasks = rooms.Select(room =>
                    //    Clients.Group(room).UserDisconnected(userId)
                    //);
                    //await Task.WhenAll(notificationTasks);
                }

                _logger.LogInformation("User {UserId} (Connection {ConnectionId}) disconnected.", userId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
