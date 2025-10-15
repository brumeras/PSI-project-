namespace KNOTS.Services;

public class StatisticsService {
    private readonly SwipeRepository _swipeRepository;
    public StatisticsService(SwipeRepository swipeRepository) { _swipeRepository = swipeRepository; }
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
    public void LogStatistics(string roomCode) {
        var stats = GetRoomStatistics(roomCode);
        Console.WriteLine($"[Room {roomCode}] Total: {stats.TotalSwipes}, Players: {stats.UniquePlayers}, Right: {stats.RightSwipes}, Left: {stats.LeftSwipes}");
    }
}