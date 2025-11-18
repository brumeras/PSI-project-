namespace KNOTS.Hubs;

public class PlayerStatus {
    public string Username { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime Timestamp { get; set; }
}