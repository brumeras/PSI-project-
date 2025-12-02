using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNOTS.Models;
using KNOTS.Services.Chat;
using Microsoft.AspNetCore.SignalR;

namespace KNOTS.Hubs
{
    public class ChatHub : Hub
    {
        // normalizedUsername -> connectionIds
        private static readonly ConcurrentDictionary<string, HashSet<string>> UserToConnections = new();
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[ChatHub] Connected: conn={Context.ConnectionId}, principalName={Context.User?.Identity?.Name ?? "(null)"}");
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

        private static string Normalize(string? username) => (username ?? string.Empty).Trim().ToLowerInvariant();
        private static string GetUserGroupName(string normalizedUsername) => $"user:{normalizedUsername}";

        public async Task SetUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine($"[ChatHub] SetUsername called with empty username for conn {Context.ConnectionId}");
                await Clients.Caller.SendAsync("Error", "Username cannot be empty.");
                return;
            }

            var normalized = Normalize(username);
            var groupName = GetUserGroupName(normalized);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var set = UserToConnections.GetOrAdd(normalized, _ => new HashSet<string>());
            lock (set) { set.Add(Context.ConnectionId); }

            Console.WriteLine($"[ChatHub] SetUsername: conn={Context.ConnectionId} -> username='{username}' normalized='{normalized}'. Group={groupName}. ConnectionCount={set.Count}");

            // Optional confirmation
            await Clients.Caller.SendAsync("UsernameConfirmed", new { username, normalized });
        }

        public Task<string> WhoAmI()
        {
            var normalized = GetUsernameByConnection(Context.ConnectionId);
            Console.WriteLine($"[ChatHub] WhoAmI: conn={Context.ConnectionId} returning '{normalized ?? "(none)"}'");
            return Task.FromResult(normalized ?? string.Empty);
        }

        public async Task SendChatMessage(string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(receiverId) || string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"[ChatHub] SendChatMessage invalid args: receiver='{receiverId}', contentLength={(content?.Length ?? 0)}");
                await Clients.Caller.SendAsync("Error", "Receiver and content required.");
                return;
            }

            var senderNormalized = GetUsernameByConnection(Context.ConnectionId);
            if (string.IsNullOrWhiteSpace(senderNormalized))
            {
                Console.WriteLine($"[ChatHub] SendChatMessage: ERROR - No username set for conn {Context.ConnectionId}. Call SetUsername first.");
                await Clients.Caller.SendAsync("Error", "Set username before sending messages.");
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

            Console.WriteLine($"[ChatHub] SendChatMessage: sender='{senderNormalized}' -> receiver='{receiverNormalized}' preview='{(content.Length > 80 ? content[..80] + "..." : content)}'");

            try
            {
                await _messageService.SendMessage(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] SendChatMessage: EXCEPTION while persisting/broadcasting: {ex}");
                await Clients.Caller.SendAsync("Error", "Failed to send message.");
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
                        if (set.Count == 0)
                            UserToConnections.TryRemove(kv.Key, out _);
                        normalizedUsername = kv.Key;
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
                        return kv.Key;
                }
            }
            return null;
        }
    }
}