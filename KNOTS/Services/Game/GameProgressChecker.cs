namespace KNOTS.Services;

public class GameProgressChecker {
    private readonly SwipeRepository _swipeRepository;
    private readonly StatisticsService _statisticsService;
    public GameProgressChecker(SwipeRepository swipeRepository, StatisticsService statisticsService) {
        _swipeRepository = swipeRepository;
        _statisticsService = statisticsService;
    }
    public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements)
    {
        if (playerUsernames == null || !playerUsernames.Any())
        {
            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: No players provided");
            return false;
        }

        if (_swipeRepository == null) throw new Exception("SwipeRepository not injected!");
        if (_statisticsService == null) throw new Exception("StatisticsService not injected!");

        var roomSwipes = _swipeRepository.GetRoomSwipes(roomCode) ?? new List<PlayerSwipe>();
        var stats = _statisticsService.GetRoomStatistics(roomCode);

        if (stats == null)
        {
            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: No statistics found");
            return false;
        }

        Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: {stats.UniquePlayers} players, {stats.TotalSwipes} total swipes, Expected: {totalStatements} statements per player");

        foreach (var player in playerUsernames)
        {
            var playerSwipeCount = roomSwipes.Count(s => s.PlayerUsername == player);
            Console.WriteLine($"[HaveAllPlayersFinished] Player {player}: {playerSwipeCount}/{totalStatements} swipes");

            if (playerSwipeCount < totalStatements) return false;
        }

        Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: All players finished!");
        return true;
    }

}