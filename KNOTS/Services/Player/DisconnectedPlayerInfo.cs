namespace KNOTS.Services;

/// <summary>
/// Represents information about a player who has disconnected from a game room.
/// </summary>
/// <remarks>
/// Used to identify which player has disconnected and from which room,
/// allowing the system to update the room state or reassign the host if needed.
/// </remarks>
public struct DisconnectedPlayerInfo
{
    /// <summary>
    /// Gets or sets the username of the disconnected player.
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Gets or sets the room code of the room the player was in before disconnecting.
    /// </summary>
    public string RoomCode { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DisconnectedPlayerInfo"/> struct.
    /// </summary>
    /// <param name="username">The username of the disconnected player.</param>
    /// <param name="roomCode">The room code of the room the player was in.</param>
    public DisconnectedPlayerInfo(string username, string roomCode) {
        Username = username;
        RoomCode = roomCode;
    }
}