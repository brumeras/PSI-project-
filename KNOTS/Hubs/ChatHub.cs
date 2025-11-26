// Hubs/ChatHub.cs
using KNOTS.Data;
using KNOTS.Models;
using Microsoft.AspNetCore.SignalR;

namespace KNOTS.Hubs;

public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    
    public ChatHub(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task SendMessage(string senderUsername, string receiverUsername, string message)
    {
        var msg = new Message
        {
            SenderId = senderUsername,
            ReceiverId = receiverUsername,
            Content = message,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        
        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();
        
        // Siųsti žinutę tik gavėjui
        await Clients.User(receiverUsername)
            .SendAsync("ReceiveMessage", senderUsername, message, msg.SentAt);
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