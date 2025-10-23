namespace KNOTS.Services;

/// <summary>
/// Checks whether all players in a given game room have completed their swipes.
/// </summary>
/// <remarks>
/// The <see cref="GameProgressChecker"/> uses both the <see cref="SwipeRepository"/> 
/// and <see cref="StatisticsService"/> to determine player progress based on 
/// the total number of expected statements.
/// </remarks>
public class GameProgressChecker {
    private readonly SwipeRepository _swipeRepository;
    private readonly StatisticsService _statisticsService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameProgressChecker"/> class.
    /// </summary>
    /// <param name="swipeRepository">Repository used to retrieve player swipes.</param>
    /// <param name="statisticsService">Service used to access overall room statistics.</param>
    public GameProgressChecker(SwipeRepository swipeRepository, StatisticsService statisticsService) {
        _swipeRepository = swipeRepository;
        _statisticsService = statisticsService;
    }
    
    /// <summary>
    /// Determines whether all players in the given room have finished swiping on all statements.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the game room.</param>
    /// <param name="playerUsernames">A list of all participating player usernames.</param>
    /// <param name="totalStatements">The total number of statements that each player is expected to swipe on.</param>
    /// <returns>
    /// <see langword="true"/> if every player has swiped on all statements; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method also logs progress information for debugging purposes,
    /// including swipe counts and expected totals for each player.
    /// </remarks>
    public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements) {
        if (playerUsernames == null || !playerUsernames.Any()) {
            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: No players provided");
            return false;
        }
        var roomSwipes = _swipeRepository.GetRoomSwipes(roomCode);
        var stats = _statisticsService.GetRoomStatistics(roomCode);
            
        Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: {stats.UniquePlayers} players, {stats.TotalSwipes} total swipes, Expected: {totalStatements} statements per player");

        foreach (var player in playerUsernames) {
            var playerSwipeCount = roomSwipes.Count(s => s.PlayerUsername == player);
            Console.WriteLine($"[HaveAllPlayersFinished] Player {player}: {playerSwipeCount}/{totalStatements} swipes");
                
            if (playerSwipeCount < totalStatements) { return false; }
        }
        Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: All players finished!");
        return true;
    }
}