using System;

namespace KNOTS.Services;

/// <summary>
/// Represents a player participating in a game room.
/// </summary>
/// <remarks>
/// Stores essential information about a connected player, including
/// their connection ID, username, join time, and readiness state.
/// </remarks>
public struct GamePlayer {
    /// <summary>
    /// Gets or sets the unique connection identifier associated with the player.
    /// </summary>
    /// <remarks>
    /// Typically used to track players in real-time sessions or WebSocket connections.
    /// </remarks>
    public string ConnectionId { get; set; }
    
    /// <summary>
    /// Gets or sets the player's chosen username.
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp indicating when the player joined the room.
    /// </summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the player is ready to start the game.
    /// </summary>
    public bool IsReady { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GamePlayer"/> struct
    /// with the specified connection ID and username.
    /// </summary>
    /// <param name="connectionId">The unique connection identifier of the player.</param>
    /// <param name="username">The player's username.</param>
    public GamePlayer(string connectionId, string username) {
        ConnectionId = connectionId;
        Username = username;
        JoinedAt = DateTime.Now;
        IsReady = false;
    }
}