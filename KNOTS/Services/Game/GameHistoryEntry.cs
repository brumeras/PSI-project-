using KNOTS.Compability;

namespace KNOTS.Services;

/// <summary>
/// Represents a record of a completed game session,
/// including room details, date, player count, and compatibility results.
/// </summary>
public class GameHistoryEntry {
    /// <summary>
    /// The unique code identifying the room where the game was played.
    /// </summary>
    public string RoomCode { get; set; } = "";
    
    /// <summary>
    /// The date and time when the game was played.
    /// </summary>
    public DateTime PlayedDate { get; set; }
    
    /// <summary>
    /// Total number of players who participated in the game.
    /// </summary>
    public int TotalPlayers { get; set; }
    
    /// <summary>
    /// The username of the player with the highest compatibility score.
    /// </summary>
    public string BestMatchPlayer { get; set; } = "";
    
    /// <summary>
    /// The highest compatibility percentage achieved in the game.
    /// </summary>
    public double BestMatchPercentage { get; set; }
    
    /// <summary>
    /// A list of all compatibility results calculated for this game.
    /// </summary>
    public List<CompatibilityScore> AllResults { get; set; } = new();
}