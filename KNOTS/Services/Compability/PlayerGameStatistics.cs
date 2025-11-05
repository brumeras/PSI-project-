namespace KNOTS.Services.Compability;

/// <summary>
/// Represents summarized game statistics for a single player during one game session.
/// </summary>
/// <remarks>
/// This model stores general performance metrics such as average compatibility,
/// best match data, and participation count.
/// </remarks>
public class PlayerGameStatistics {
    /// <summary>
    /// Gets or sets the username of the player.
    /// </summary>
    public string PlayerUsername { get; set; } = "";
    /// <summary>
    /// Gets or sets the number of games played by the player.
    /// </summary>
    public int GamesPlayed { get; set; }
    /// <summary>
    /// Gets or sets the player’s average compatibility percentage across all matches.
    /// </summary>
    public double AverageCompatibility { get; set; }
    /// <summary>
    /// Gets or sets the player’s best match percentage achieved in the session.
    /// </summary>
    public double BestMatchPercentage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the player was the best match for another participant.
    /// </summary>
    public bool WasBestMatch { get; set; }
}