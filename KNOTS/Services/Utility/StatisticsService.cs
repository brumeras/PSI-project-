namespace KNOTS.Services;

/// <summary>
/// Service responsible for calculating and logging statistics for game rooms.
/// </summary>
public class StatisticsService {
    private readonly SwipeRepository _swipeRepository;
    
    /// <summary>
    /// Initializes a new instance of <see cref="StatisticsService"/> with the specified swipe repository.
    /// </summary>
    /// <param name="swipeRepository">Repository for accessing player swipes.</param>
    public StatisticsService(SwipeRepository swipeRepository) { _swipeRepository = swipeRepository; }
    
    
    /// <summary>
    /// Calculates statistics for a given room based on the swipes of its players.
    /// </summary>
    /// <param name="roomCode">The code of the room to calculate statistics for.</param>
    /// <returns>A <see cref="RoomStatistics"/> object containing total swipes, unique players, unique statements, and swipe counts.</returns>
    public RoomStatistics GetRoomStatistics(string roomCode) {
        var roomSwipes = _swipeRepository.GetRoomSwipes(roomCode);
        return new RoomStatistics {
            TotalSwipes = roomSwipes.Count,
            UniquePlayers = roomSwipes.Select(s => s.PlayerUsername).Distinct().Count(),
            UniqueStatements = roomSwipes.Select(s => s.StatementId).Distinct().Count(),
            RightSwipes = roomSwipes.Count(s => s.AgreeWithStatement),
            LeftSwipes = roomSwipes.Count(s => !s.AgreeWithStatement)
        };
    }
    
    /// <summary>
    /// Logs statistics for a given room to the console.
    /// </summary>
    /// <param name="roomCode">The code of the room to log statistics for.</param>
    public void LogStatistics(string roomCode) {
        var stats = GetRoomStatistics(roomCode);
        Console.WriteLine($"[Room {roomCode}] Total: {stats.TotalSwipes}, Players: {stats.UniquePlayers}, Right: {stats.RightSwipes}, Left: {stats.LeftSwipes}");
    }
}