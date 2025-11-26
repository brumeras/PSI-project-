using KNOTS.Models;

namespace KNOTS.Services.Chat;

public interface IMessageService
{
    Task<List<Message>> GetConversation(string username1, string username2);
    Task<List<Conversation>> GetUserConversations(string username);
    Task<int> GetUnreadCount(string username);
    Task SendMessage(Message message);
    Task MarkAsRead(int messageId);
}