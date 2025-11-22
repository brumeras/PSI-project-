// Services/MessageService.cs
using KNOTS.Data;
using KNOTS.Models;
using Microsoft.EntityFrameworkCore;

namespace KNOTS.Services.Chat;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;
    
    public MessageService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Message>> GetConversation(string username1, string username2)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == username1 && m.ReceiverId == username2) ||
                        (m.SenderId == username2 && m.ReceiverId == username1))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
    
    public async Task<List<Conversation>> GetUserConversations(string username)
    {
        var messages = await _context.Messages
            .Where(m => m.SenderId == username || m.ReceiverId == username)
            .GroupBy(m => m.SenderId == username ? m.ReceiverId : m.SenderId)
            .Select(g => new Conversation
            {
                User1Username = username,
                User2Username = g.Key,
                LastMessageAt = g.Max(m => m.SentAt),
                Messages = g.OrderByDescending(m => m.SentAt).Take(1).ToList()
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
            
        return messages;
    }
    
    public async Task<int> GetUnreadCount(string username)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == username && !m.IsRead);
    }
    
    public async Task SendMessage(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }
    
    public async Task MarkAsRead(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}