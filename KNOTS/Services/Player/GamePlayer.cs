using System;

namespace KNOTS.Services;
public struct GamePlayer {
    public string ConnectionId { get; set; }
    public string Username { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsReady { get; set; }
    public GamePlayer(string connectionId, string username) {
        ConnectionId = connectionId;
        Username = username;
        JoinedAt = DateTime.Now;
        IsReady = false;
    }
}