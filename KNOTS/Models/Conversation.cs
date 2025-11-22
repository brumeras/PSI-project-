namespace KNOTS.Models;

public class Conversation
{
    public string User1Username { get; set; } = string.Empty;
    public string User2Username { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
    public DateTime LastMessageAt { get; set; }
}