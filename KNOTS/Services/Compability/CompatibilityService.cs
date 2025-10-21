
using KNOTS.Compability;
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Compability;

public class CompatibilityService
{
    private readonly StatementRepository _statements;
    private readonly SwipeRepository _swipes;
    private readonly CompatibilityCalculator _calculator;
    private readonly GameHistoryService _history;
    private readonly StatisticsService _stats;
    private readonly GameProgressChecker _gameProgressChecker;
    private readonly GameHistoryRepository _gameHistoryRepository;
    public CompatibilityService(AppDbContext context, UserService userService, GameHistoryRepository gameHistoryRepository, GameProgressChecker gameProgressChecker )
    {
        _statements = new StatementRepository(context);
        _swipes = new SwipeRepository(context);
        _calculator = new CompatibilityCalculator(_swipes);
        _history = new GameHistoryService(context, userService, _calculator);
        _stats = new StatisticsService(context);
        _gameHistoryRepository = gameHistoryRepository;
        _gameProgressChecker = gameProgressChecker;
        _statements.EnsureDefaultStatements();
    }

    public List<GameStatement> GetRandomStatements(int count) =>
        _statements.GetAllStatements().OrderBy(_ => Guid.NewGuid()).Take(count).ToList();

    public bool SaveSwipe(string room, string player, string statementId, bool agree) =>
        _swipes.SaveSwipe(room, player, statementId, agree);

    public CompatibilityScore CalculateCompatibility(string room, string p1, string p2) =>
        _calculator.Calculate(room, p1, p2);

    public void SaveGameToHistory(string room, List<string> players) =>
        _history.SaveGame(room, players);

    public RoomStatistics GetRoomStats(string room) =>
        _stats.GetRoomStatistics(room);

    public void ClearRoomData(string roomCode) { _swipes.ClearRoomData(roomCode); }
    public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> players) {
        var results = new List<CompatibilityScore>();

        for (int i = 0; i < players.Count; i++) {
            for (int j = i + 1; j < players.Count; j++) {
                var score = CalculateCompatibility(roomCode, players[i], players[j]);
                results.Add(score);
            }
        }
        return results.OrderByDescending(r => r.Percentage).ToList();
    }
    public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) { return _swipes.GetPlayerSwipes(roomCode, playerUsername); }
    public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements) { return _gameProgressChecker.HaveAllPlayersFinished(roomCode, playerUsernames, totalStatements); }
    public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) { return _gameHistoryRepository.GetPlayerHistory(playerUsername); }
}



