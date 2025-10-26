using System.Linq;
using KNOTS.Data;

namespace KNOTS.Services;

public class StatisticsService {
    private readonly AppDbContext _context;
    public StatisticsService(AppDbContext context) { _context = context; }
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
