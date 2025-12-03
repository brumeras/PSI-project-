using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNOTS.Models;
using KNOTS.Services.Chat;
using Microsoft.AspNetCore.SignalR;

namespace KNOTS.Hubs
{
    public class ChatHub : Hub
    {
        // Use a normalized key for routing (trim + lowercase).
        // Map normalizedUsername -> set of connectionIds
        private static readonly ConcurrentDictionary<string, HashSet<string>> UserToConnections = new();
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[ChatHub] Connected: conn={Context.ConnectionId}, principalName={Context.User?.Identity?.Name}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (TryRemoveConnection(Context.ConnectionId, out var normalizedUsername))
            {
                var groupName = GetUserGroupName(normalizedUsername);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                Console.WriteLine($"[ChatHub] Disconnected: conn={Context.ConnectionId} removed from normalizedUser={normalizedUsername} group={groupName}");
            }
            else
            {
                Console.WriteLine($"[ChatHub] Disconnected: conn={Context.ConnectionId} had no mapped username");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Normalize: trim and lowercase to keep group keys consistent.
        private static string Normalize(string? username) => (username ?? string.Empty).Trim().ToLowerInvariant();

        private static string GetUserGroupName(string normalizedUsername) => $"user:{normalizedUsername}";

        public async Task SetUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine($"[ChatHub] SetUsername called with empty username for conn {Context.ConnectionId}");
                return;
            }

            var normalized = Normalize(username);
            var groupName = GetUserGroupName(normalized);
    
            // Add to group FIRST (idempotent)
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    
            // Then track connection
            AddConnection(normalized, Context.ConnectionId);

            Console.WriteLine($"[ChatHub] SetUsername: conn={Context.ConnectionId} -> username='{username}' normalized='{normalized}'. Added to group {groupName}. ConnectionCount={GetConnectionCount(normalized)}");
        }

        // Debug helper for client to confirm what server thinks about this connection
        public Task<string> WhoAmI()
        {
            var normalized = GetUsernameByConnection(Context.ConnectionId);
            Console.WriteLine($"[ChatHub] WhoAmI: conn={Context.ConnectionId} returning normalized='{normalized ?? "(none)"}'");
            return Task.FromResult(normalized ?? string.Empty);
        }

        public async Task SendChatMessage(string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(receiverId) || string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"[ChatHub] SendChatMessage invalid args: receiver='{receiverId}', contentLength={(content?.Length ?? 0)}");
                return;
            }

            // STRICT: Must have called SetUsername first
            var senderNormalized = GetUsernameByConnection(Context.ConnectionId);
            if (string.IsNullOrWhiteSpace(senderNormalized))
            {
                Console.WriteLine($"[ChatHub] SendChatMessage: ERROR - No username set for conn {Context.ConnectionId}. Call SetUsername first!");
                await Clients.Caller.SendAsync("Error", "Username not set. Please set username before sending messages.");
                return;
            }

            var receiverNormalized = Normalize(receiverId);

            var message = new Message
            {
                SenderId = senderNormalized,
                ReceiverId = receiverNormalized,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            Console.WriteLine($"[ChatHub] SendChatMessage: conn={Context.ConnectionId} sender='{senderNormalized}' -> receiver='{receiverNormalized}', contentPreview='{(content.Length > 80 ? content.Substring(0, 80) + "..." : content)}'");

            await _messageService.SendMessage(message);

            Console.WriteLine($"[ChatHub] SendChatMessage: persisted and broadcast attempted to groups '{GetUserGroupName(receiverNormalized)}' and '{GetUserGroupName(senderNormalized)}'");
        }

        // ----- helpers for connection tracking -----
        private static void AddConnection(string normalizedUsername, string connectionId)
        {
            var set = UserToConnections.GetOrAdd(normalizedUsername, _ => new HashSet<string>());
            lock (set)
            {
                set.Add(connectionId);
            }
        }

// Add explicit removal (not just in OnDisconnected)
        private static void RemoveUserIfEmpty(string normalizedUsername)
        {
            if (UserToConnections.TryGetValue(normalizedUsername, out var set))
            {
                lock (set)
                {
                    if (set.Count == 0)
                    {
                        UserToConnections.TryRemove(normalizedUsername, out _);
                    }
                }
            }
        }

        private static bool TryRemoveConnection(string connectionId, out string normalizedUsername)
        {
            normalizedUsername = null!;
            foreach (var kv in UserToConnections)
            {
                var set = kv.Value;
                lock (set)
                {
                    if (set.Remove(connectionId))
                    {
                        normalizedUsername = kv.Key;
                        if (set.Count == 0)
                        {
                            UserToConnections.TryRemove(kv.Key, out _);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static string? GetUsernameByConnection(string connectionId)
        {
            foreach (var kv in UserToConnections)
            {
                var set = kv.Value;
                lock (set)
                {
                    if (set.Contains(connectionId))
                    {
                        return kv.Key;
                    }
                }
            }
            return null;
        }

        private static int GetConnectionCount(string normalizedUsername)
        {
            if (UserToConnections.TryGetValue(normalizedUsername, out var set))
            {
                lock (set) { return set.Count; }
            }
            return 0;
        }
    }
}