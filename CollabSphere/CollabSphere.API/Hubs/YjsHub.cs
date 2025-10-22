using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.API.Hubs
{
    [Authorize]
    public class YjsHub : Hub
    {
        // Dictionary<roomName, YDocState>
        private static readonly ConcurrentDictionary<string, List<string>> DocumentStates = new();
        private static readonly ConcurrentDictionary<string, int> UserConnectionIds = new();

        // When a client joins a "room"
        public async Task JoinDocument(string room)
        {
            try
            {
                var userIdClaim = Context.User!.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);
                UserConnectionIds.TryAdd(Context.ConnectionId, userId);

                await Groups.AddToGroupAsync(Context.ConnectionId, room);
                var notifyConnect = Clients.Groups(room).SendAsync("UserConnected", userId);

                if (DocumentStates.TryGetValue(room, out var state))
                {
                    // Send current document state to new client
                    await Clients.Caller.SendAsync("ReceiveDocState", state);
                }
            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }
        }

        // Broadcast Yjs document updates
        public async Task BroadcastUpdate(string room, string updateBase64)
        {
            try
            {
                var list = DocumentStates.GetOrAdd(room, _ => new List<string>());
                list.Add(updateBase64);

                // Forward to others in the room
                var sendDocUpdate = Clients.OthersInGroup(room).SendAsync("ReceiveUpdate", updateBase64);
            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }
        }

        // Broadcast Yjs awareness updates (cursor positions, selections, etc.)
        public async Task BroadcastAwareness(string room, string updateBase64)
        {
            try
            {
                var sendAwareness = Clients.OthersInGroup(room).SendAsync("ReceiveAwareness", updateBase64);
            }
            catch (Exception ex)
            {
                var test = ex.InnerException;
            }
        }

        public async Task DisconnectFromDocument(string room)
        {
            await Groups.RemoveFromGroupAsync(room, Context.ConnectionId);

            await OnDisconnectedAsync(null);
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
            // Optional: cleanup logic if needed
            if (UserConnectionIds.TryGetValue(Context.ConnectionId, out var userId))
            {
                Console.WriteLine($"User {userId} disconnected.");

                // Perform cleanup (e.g., mark user as offline, remove from room, etc.)
                var notifyDisconnect = Clients.All.SendAsync("UserDisconnected", userId);
                if (UserConnectionIds.TryRemove(new KeyValuePair<string, int>(Context.ConnectionId, userId)))
                {
                    Console.WriteLine($"Removed connection '{Context.ConnectionId}' of user '{userId}'");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
