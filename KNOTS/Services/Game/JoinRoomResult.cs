namespace KNOTS.Services;

/// <summary>
/// Represents the result of attempting to join a <see cref="GameRoom"/>.
/// </summary>
/// <remarks>
/// Contains information about whether the join was successful, a message describing the result,
/// and the current state of the room at the time of the attempt.
/// </remarks>
public struct JoinRoomResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the join attempt was successful.
    /// </summary>
    public bool Success { get; set; }
    
    
    /// <summary>
    /// Gets or sets a descriptive message explaining the outcome of the join attempt.
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Gets or sets the current <see cref="GameState"/> of the room.
    /// </summary>
    public GameState State { get; set; }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinRoomResult"/> struct.
    /// </summary>
    /// <param name="success">Indicates whether the join attempt was successful.</param>
    /// <param name="message">A message describing the result of the attempt.</param>
    /// <param name="state">The current state of the room. Defaults to <see cref="GameState.WaitingForPlayers"/>.</param>
    public JoinRoomResult(bool success, string message, GameState state = GameState.WaitingForPlayers)
    {
        Success = success;
        Message = message;
        State = state;
    }
}
