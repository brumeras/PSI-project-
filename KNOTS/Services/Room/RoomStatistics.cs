namespace KNOTS.Services;

/// <summary>
/// Represents aggregated statistics for a game room.
/// </summary>
/// <remarks>
/// Tracks the total number of swipes, unique players and statements,
/// and the count of right and left swipes.
/// </remarks>
public class RoomStatistics {
    
    /// <summary>Total number of swipes made in the room.</summary>
    public int TotalSwipes { get; set; }
    
    /// <summary>Number of unique players who participated.</summary>
    public int UniquePlayers { get; set; }
    
    /// <summary>Number of unique statements encountered.</summary>
    public int UniqueStatements { get; set; }
    
    
    /// <summary>Count of right swipes (agreements).</summary>
    public int RightSwipes { get; set; }
    
    /// <summary>Count of left swipes (disagreements).</summary>
    public int LeftSwipes { get; set; }
}
