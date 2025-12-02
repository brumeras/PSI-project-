using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNOTS.Data;
using KNOTS.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using KNOTS.Hubs;

namespace KNOTS.Services.Chat
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hub;

        public MessageService(AppDbContext context, IHubContext<ChatHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        private static string Normalize(string? username) => (username ?? string.Empty).Trim().ToLowerInvariant();

        public async Task<List<Message>> GetConversation(string username1, string username2)
        {
            var n1 = Normalize(username1);
            var n2 = Normalize(username2);

            return await _context.Messages
                .Where(m => (m.SenderId == n1 && m.ReceiverId == n2) ||
                            (m.SenderId == n2 && m.ReceiverId == n1))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetUserConversations(string username)
        {
            var n = Normalize(username);

            return await _context.Messages
                .Where(m => m.SenderId == n || m.ReceiverId == n)
                .GroupBy(m => m.SenderId == n ? m.ReceiverId : m.SenderId)
                .Select(g => new Conversation
                {
                    User1Username = n,
                    User2Username = g.Key,
                    LastMessageAt = g.Max(m => m.SentAt),
                    Messages = g.OrderByDescending(m => m.SentAt).Take(1).ToList()
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCount(string username)
        {
            var n = Normalize(username);
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == n && !m.IsRead);
        }

        public async Task SendMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.SenderId = Normalize(message.SenderId);
            message.ReceiverId = Normalize(message.ReceiverId);
            if (message.SentAt == default) message.SentAt = DateTime.UtcNow;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var receiverGroup = $"user:{message.ReceiverId}";
            var senderGroup = $"user:{message.SenderId}";

            Console.WriteLine($"[MessageService] Saved message id={message.Id} from={message.SenderId} to={message.ReceiverId}. Broadcasting to groups '{receiverGroup}' AND '{senderGroup}' via ReceiveMessage.");

            // Single unified event for both participants.
            await _hub.Clients.Groups(receiverGroup, senderGroup)
                .SendAsync("ReceiveMessage", new
                {
                    id = message.Id,
                    senderId = message.SenderId,
                    receiverId = message.ReceiverId,
                    content = message.Content,
                    sentAt = message.SentAt,
                    isRead = message.IsRead
                });
        }

        public async Task MarkAsRead(int messageId)
        {
            var msg = await _context.Messages.FindAsync(messageId);
            if (msg != null && !msg.IsRead)
            {
                msg.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkConversationAsRead(string username1, string username2)
        {
            var n1 = Normalize(username1);
            var n2 = Normalize(username2);

            var unread = await _context.Messages
                .Where(m => m.ReceiverId == n1 && m.SenderId == n2 && !m.IsRead)
                .ToListAsync();

            if (unread.Count == 0) return;

            foreach (var m in unread) m.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}