namespace KNOTS.Services;

public struct DisconnectedPlayerInfo {
    public string Username { get; set; }
    public string RoomCode { get; set; }
    
    public DisconnectedPlayerInfo(string username, string roomCode) {
        Username = username;
        RoomCode = roomCode;
    }
}