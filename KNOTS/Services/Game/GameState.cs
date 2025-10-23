namespace KNOTS.Services;

/// <summary>
/// Represents the current state of a <see cref="GameRoom"/>.
/// </summary>
public enum GameState
{
    /// <summary>
    /// The game is waiting for players to join; not started yet.
    /// </summary>
    WaitingForPlayers,
    
    /// <summary>
    /// The game is currently in progress.
    /// </summary>
    InProgress,
    
    /// <summary>
    /// The game has finished.
    /// </summary>
    Finished
}
