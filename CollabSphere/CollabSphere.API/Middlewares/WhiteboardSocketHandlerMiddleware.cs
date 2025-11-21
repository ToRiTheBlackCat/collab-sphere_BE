using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CollabSphere.API.Middlewares
{
    public class WhiteboardSocketHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        // Tracks users by Page (for shape/cursor sync)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> Pages = new();

        // Tracks users by Whiteboard (for new page/rename events)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> Whiteboards = new();

        public WhiteboardSocketHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, collab_sphereContext dbContext)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            
            var pageIdStr = context.Request.Query["pageId"].ToString();
            var whiteboardIdStr = context.Request.Query["whiteboardId"].ToString();  
            var drawerIdStr = context.Request.Query["drawerId"].ToString();
            var userName = context.Request.Query["userName"].ToString();

            if (!int.TryParse(pageIdStr, out var pageId) ||
                !int.TryParse(whiteboardIdStr, out var whiteboardId) || 
                !int.TryParse(drawerIdStr, out var drawerId) ||
                string.IsNullOrEmpty(userName))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("A valid 'pageId' (int), 'whiteboardId' (int), 'drawerId' (int), and 'userName' (string) are required.");
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var pageKey = pageId.ToString();
            var whiteboardKey = whiteboardId.ToString(); // Store this key

            Console.WriteLine($"✅ {userName} (ID: {drawerId}) joined whiteboard '{whiteboardKey}', page '{pageKey}'");
       
            var pageSockets = Pages.GetOrAdd(pageKey, _ => new ConcurrentDictionary<string, WebSocket>());
            pageSockets.TryAdd(drawerIdStr, socket);

            var whiteboardSockets = Whiteboards.GetOrAdd(whiteboardKey, _ => new ConcurrentDictionary<string, WebSocket>());
            whiteboardSockets.TryAdd(drawerIdStr, socket); 

            // Send initial shapes
            await SendInitialState(socket, pageId, dbContext);

            try
            {
                var buffer = new byte[1024 * 8];
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (string.IsNullOrWhiteSpace(message)) continue;

                    if (message.Contains("\"type\":\"sync\""))
                    {
                        await UpdateDatabaseState(pageId, drawerId, message, dbContext);
                    }

                    // Broadcast shape/cursor messages ONLY to users on the same PAGE
                    await BroadcastToPage(pageKey, drawerIdStr, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Socket error for {userName} (ID: {drawerId}): {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"🚪 {userName} (ID: {drawerId}) is disconnecting...");

                if (Pages.TryGetValue(pageKey, out var pSockets))
                {
                    if (pSockets.TryRemove(drawerIdStr, out _))
                    {
                        var leaveMessage = new
                        {
                            type = "leave",
                            userId = drawerIdStr,
                            userName = userName,
                            roomId = pageKey
                        };
                        await BroadcastToPage(pageKey, drawerIdStr, JsonSerializer.Serialize(leaveMessage));
                    }
                    if (pSockets.IsEmpty) Pages.TryRemove(pageKey, out _);
                }

                if (Whiteboards.TryGetValue(whiteboardKey, out var wbSockets))
                {
                    wbSockets.TryRemove(drawerIdStr, out _);
                    if (wbSockets.IsEmpty) Whiteboards.TryRemove(whiteboardKey, out _);
                }

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }

        private async Task BroadcastToPage(string pageKey, string senderDrawerId, string message)
        {
            if (Pages.TryGetValue(pageKey, out var pageSockets))
            {
                var msgBytes = Encoding.UTF8.GetBytes(message);
                var sendTasks = pageSockets
                    .Where(p => p.Key != senderDrawerId && p.Value.State == WebSocketState.Open)
                    .Select(p => p.Value.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None));
                await Task.WhenAll(sendTasks);
            }
        }

        public static async Task BroadcastNewPageAsync(int whiteboardId, Domain.Entities.WhiteboardPage newPage)
        {
            var whiteboardKey = whiteboardId.ToString();

            var messagePayload = new
            {
                type = "new_page",
                page = new
                {
                    pageId = newPage.PageId,
                    pageTitle = newPage.PageTitle,
                    isActive = newPage.IsActivate
                }
            };
            var message = JsonSerializer.Serialize(messagePayload);
            var msgBytes = Encoding.UTF8.GetBytes(message);

            if (Whiteboards.TryGetValue(whiteboardKey, out var whiteboardSockets))
            {
                var sendTasks = whiteboardSockets.Values
                    .Where(socket => socket.State == WebSocketState.Open)
                    .Select(socket => socket.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None));

                await Task.WhenAll(sendTasks);
                Console.WriteLine($"📬 Broadcasted new page '{newPage.PageTitle}' to whiteboard {whiteboardId}.");
            }
        }

        public static async Task BroadcastUpdatePageAsync(int whiteboardId, Domain.Entities.WhiteboardPage updatedPage)
        {
            var whiteboardKey = whiteboardId.ToString();

            var messagePayload = new
            {
                type = "update_page",
                page = new
                {
                    pageId = updatedPage.PageId,
                    pageTitle = updatedPage.PageTitle
                }
            };
            var message = JsonSerializer.Serialize(messagePayload);
            var msgBytes = Encoding.UTF8.GetBytes(message);

            if (Whiteboards.TryGetValue(whiteboardKey, out var whiteboardSockets))
            {
                var sendTasks = whiteboardSockets.Values
                    .Where(socket => socket.State == WebSocketState.Open)
                    .Select(socket => socket.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None));

                await Task.WhenAll(sendTasks);
                Console.WriteLine($"📬 Broadcasted page update for '{updatedPage.PageTitle}' to whiteboard {whiteboardId}.");
            }
        }

        public static async Task BroadcastDeletePageAsync(int whiteboardId, int deletedPageId)
        {
            var whiteboardKey = whiteboardId.ToString();

            var messagePayload = new
            {
                type = "delete_page",
                page = new
                {
                    pageId = deletedPageId
                }
            };
            var message = JsonSerializer.Serialize(messagePayload);
            var msgBytes = Encoding.UTF8.GetBytes(message);

            if (Whiteboards.TryGetValue(whiteboardKey, out var whiteboardSockets))
            {
                var sendTasks = whiteboardSockets.Values
                    .Where(socket => socket.State == WebSocketState.Open)
                    .Select(socket => socket.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None));

                await Task.WhenAll(sendTasks);
                Console.WriteLine($"📬 Broadcasted page deletion {deletedPageId} to whiteboard {whiteboardId}.");
            }
        }

        private async Task SendInitialState(WebSocket socket, int pageId, collab_sphereContext dbContext)
        {
            var shapes = await dbContext.Shapes
                .Where(s => s.PageId == pageId)
                .AsNoTracking()
                .ToListAsync();

            // ---- 1. Build a *real* object dictionary ----
            var added = new Dictionary<string, object>();

            foreach (var s in shapes)
            {
                // Parse the stored JSON string → JsonElement (real object)
                var parsed = JsonNode.Parse(s.JsonData);
                // Convert to a plain .NET object (Dictionary<string,object>)
                var obj = JsonSerializer.Deserialize<object>(parsed.ToJsonString());
                added[s.ShapeId] = obj!;
            }

            var payload = new
            {
                type = "sync",
                payload = new
                {
                    added,
                    updated = new Dictionary<string, object>(),
                    removed = new Dictionary<string, object>()
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null });
            var bytes = Encoding.UTF8.GetBytes(json);

            await socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            Console.WriteLine($"Sent initial state: {added.Count} records to page {pageId}");
        }

        // Saves shape changes 
        private async Task UpdateDatabaseState(int pageId, int drawerId, string message, collab_sphereContext dbContext)
        {
            try
            {
                var json = JsonNode.Parse(message);
                if (json?["type"]?.GetValue<string>() != "sync") return;

                var payload = json["payload"];

                var addedNode = payload?["added"]?.AsObject() ?? new JsonObject();
                foreach (var kv in addedNode)
                {
                    var id = kv.Key;
                    var recordJson = kv.Value.ToJsonString();  

                    var existing = await dbContext.Shapes.FindAsync(id);
                    if (existing == null)
                    {
                        dbContext.Shapes.Add(new Domain.Entities.Shape
                        {
                            ShapeId = id,
                            PageId = pageId,
                            DrawerId = drawerId,
                            JsonData = recordJson,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existing.JsonData = recordJson;
                    }
                }

                var updatedNode = payload?["updated"]?.AsObject() ?? new JsonObject();
                foreach (var kv in updatedNode)
                {
                    var id = kv.Key;
                    var newJson = kv.Value[1].ToJsonString();

                    var shape = await dbContext.Shapes.FindAsync(id);
                    if (shape != null) shape.JsonData = newJson;
                }

                var removedNode = payload?["removed"]?.AsObject() ?? new JsonObject();
                foreach (var kv in removedNode)
                {
                    var id = kv.Key;
                    var shape = await dbContext.Shapes.FindAsync(id);
                    if (shape != null) dbContext.Shapes.Remove(shape);
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB sync error: {ex.Message}");
            }
        }
    }

}
