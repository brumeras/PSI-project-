using System.Linq;
using KNOTS.Data;

namespace KNOTS.Services;

/// <summary>
/// Service responsible for calculating and logging statistics for game rooms.
/// </summary>
public class StatisticsService {
    private readonly AppDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of <see cref="StatisticsService"/> with the specified swipe repository.
    /// </summary>
    /// <param name="swipeRepository">Repository for accessing player swipes.</param>
    public StatisticsService(AppDbContext context) { _context = context; }
    
    /// <summary>
    /// Calculates statistics for a given room based on the swipes of its players.
    /// </summary>
    /// <param name="roomCode">The code of the room to calculate statistics for.</param>
    /// <returns>A <see cref="RoomStatistics"/> object containing total swipes, unique players, unique statements, and swipe counts.</returns>
    public RoomStatistics GetRoomStatistics(string roomCode) {
        var swipes = _context.PlayerSwipes.Where(s => s.RoomCode == roomCode);
        return new RoomStatistics {
            TotalSwipes = swipes.Count(),
            UniquePlayers = swipes.Select(s => s.PlayerUsername).Distinct().Count(),
            UniqueStatements = swipes.Select(s => s.StatementId).Distinct().Count(),
            RightSwipes = swipes.Count(s => s.AgreeWithStatement),
            LeftSwipes = swipes.Count(s => !s.AgreeWithStatement)
        };
    }
}
