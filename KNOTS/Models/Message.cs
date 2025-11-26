
namespace KNOTS.Models;

public class Message
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty; // Username
    public string ReceiverId { get; set; } = string.Empty; // Username
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}