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

        // ✅ NEW: Configurable limits
        private const int INITIAL_BUFFER_SIZE = 1024 * 64;  // 64KB initial buffer
        private const int MAX_MESSAGE_SIZE = 1024 * 1024 * 2;  // 2MB max message size

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
            var whiteboardKey = whiteboardId.ToString();

            Console.WriteLine($"✅ {userName} (ID: {drawerId}) joined whiteboard '{whiteboardKey}', page '{pageKey}'");

            var pageSockets = Pages.GetOrAdd(pageKey, _ => new ConcurrentDictionary<string, WebSocket>());
            pageSockets.TryAdd(drawerIdStr, socket);

            var whiteboardSockets = Whiteboards.GetOrAdd(whiteboardKey, _ => new ConcurrentDictionary<string, WebSocket>());
            whiteboardSockets.TryAdd(drawerIdStr, socket);

            // Send initial shapes
            await SendInitialState(socket, pageId, dbContext);

            try
            {
                // ✅ FIX: Use dynamic buffer with proper multi-frame handling
                while (socket.State == WebSocketState.Open)
                {
                    var message = await ReceiveFullMessageAsync(socket);

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        // Socket was closed
                        break;
                    }

                    // Log message size for monitoring
                    var messageSizeKB = Encoding.UTF8.GetByteCount(message) / 1024.0;
                    if (messageSizeKB > 50)
                    {
                        Console.WriteLine($"📦 Large message received: {messageSizeKB:F1}KB from {userName}");
                    }

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
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// ✅ NEW: Properly receives complete WebSocket messages across multiple frames
        /// </summary>
        private async Task<string?> ReceiveFullMessageAsync(WebSocket socket)
        {
            var buffer = new byte[INITIAL_BUFFER_SIZE];
            var messageBuilder = new StringBuilder();
            var totalBytesReceived = 0;

            try
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return null;
                    }

                    // Append this frame's data
                    var frameText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(frameText);
                    totalBytesReceived += result.Count;

                    // ✅ Safety check: prevent memory issues
                    if (totalBytesReceived > MAX_MESSAGE_SIZE)
                    {
                        Console.WriteLine($"⚠️ Message exceeds max size ({MAX_MESSAGE_SIZE / 1024}KB), truncating...");
                        break;
                    }

                    // ✅ CRITICAL: Continue reading until EndOfMessage is true
                } while (!result.EndOfMessage);

                var fullMessage = messageBuilder.ToString();

                if (totalBytesReceived > INITIAL_BUFFER_SIZE)
                {
                    Console.WriteLine($"📊 Multi-frame message: {totalBytesReceived / 1024.0:F1}KB across {messageBuilder.Length / INITIAL_BUFFER_SIZE + 1} frames");
                }

                return fullMessage;
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"❌ WebSocket error during receive: {ex.Message}");
                return null;
            }
        }

        private async Task BroadcastToPage(string pageKey, string senderDrawerId, string message)
        {
            if (Pages.TryGetValue(pageKey, out var pageSockets))
            {
                var msgBytes = Encoding.UTF8.GetBytes(message);

                // ✅ Log large broadcasts
                if (msgBytes.Length > 50 * 1024)
                {
                    Console.WriteLine($"📡 Broadcasting large message: {msgBytes.Length / 1024.0:F1}KB to {pageSockets.Count - 1} users");
                }

                var sendTasks = pageSockets
                    .Where(p => p.Key != senderDrawerId && p.Value.State == WebSocketState.Open)
                    .Select(async p =>
                    {
                        try
                        {
                            await p.Value.SendAsync(
                                new ArraySegment<byte>(msgBytes),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Failed to broadcast to user {p.Key}: {ex.Message}");
                        }
                    });

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

            var added = new Dictionary<string, object>();

            foreach (var s in shapes)
            {
                var parsed = JsonNode.Parse(s.JsonData);
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

            Console.WriteLine($"📤 Sending initial state: {added.Count} shapes ({bytes.Length / 1024.0:F1}KB) to page {pageId}");

            await socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        // Saves shape changes 
        private async Task UpdateDatabaseState(int pageId, int drawerId, string message, collab_sphereContext dbContext)
        {
            try
            {
                var json = JsonNode.Parse(message);
                if (json?["type"]?.GetValue<string>() != "sync") return;

                var payload = json["payload"];

                var addedCount = 0;
                var updatedCount = 0;
                var removedCount = 0;

                // ✅ Process ADDED
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
                        addedCount++;
                    }
                    else
                    {
                        // ✅ Update if already exists (upsert behavior)
                        existing.JsonData = recordJson;
                        existing.DrawerId = drawerId; // Update drawer
                        updatedCount++;
                    }
                }

                // ✅ Process UPDATED
                var updatedNode = payload?["updated"]?.AsObject() ?? new JsonObject();
                foreach (var kv in updatedNode)
                {
                    var id = kv.Key;

                    // ✅ Handle both array format [from, to] and direct object
                    string newJson;
                    if (kv.Value is JsonArray arr && arr.Count >= 2)
                    {
                        newJson = arr[1].ToJsonString();
                    }
                    else
                    {
                        newJson = kv.Value.ToJsonString();
                    }

                    var shape = await dbContext.Shapes.FindAsync(id);
                    if (shape != null)
                    {
                        shape.JsonData = newJson;
                        shape.DrawerId = drawerId;
                        updatedCount++;
                    }
                    else
                    {
                        // ✅ If shape doesn't exist, create it (handle race condition)
                        dbContext.Shapes.Add(new Domain.Entities.Shape
                        {
                            ShapeId = id,
                            PageId = pageId,
                            DrawerId = drawerId,
                            JsonData = newJson,
                            CreatedAt = DateTime.UtcNow
                        });
                        addedCount++;
                    }
                }

                // ✅ Process REMOVED
                var removedNode = payload?["removed"]?.AsObject() ?? new JsonObject();
                foreach (var kv in removedNode)
                {
                    var id = kv.Key;
                    var shape = await dbContext.Shapes.FindAsync(id);
                    if (shape != null)
                    {
                        dbContext.Shapes.Remove(shape);
                        removedCount++;
                    }
                }

                await dbContext.SaveChangesAsync();

                var totalChanges = addedCount + updatedCount + removedCount;
                if (totalChanges > 0)
                {
                    Console.WriteLine($"💾 DB sync: +{addedCount} ~{updatedCount} -{removedCount} (total: {totalChanges}) for page {pageId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DB sync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
