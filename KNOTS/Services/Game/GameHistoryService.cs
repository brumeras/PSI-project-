using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KNOTS.Compability;
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services.Compability;

namespace KNOTS.Services;

/// <summary>
/// Manages the recording and getting game history data,
/// including compatibility results and player statistics updates.
/// </summary>
/// <remarks>
/// This service coordinates between the compatibility calculator,
/// statistics service, and user service to save completed game results
/// and update player records accordingly.
/// </remarks>
public class GameHistoryService {
    private readonly AppDbContext _context;
    private readonly UserService _userService;
    private readonly CompatibilityCalculator _calculator;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameHistoryService"/> class.
    /// </summary>
    /// <param name="historyRepository">The repository used for storing game history entries.</param>
    /// <param name="compatibilityCalculator">The calculator used to compute player compatibility scores.</param>
    /// <param name="statisticsService">The service responsible for logging overall game statistics.</param>
    /// <param name="userService">The service responsible for updating player-specific statistics.</param>
    public GameHistoryService(AppDbContext context, UserService userService, CompatibilityCalculator calculator) {
        _context = context;
        _userService = userService;
        _calculator = calculator;
    }
    
    /// <summary>
    /// Saves the results of a completed compatibility game session for a specific room.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room where the game was played.</param>
    /// <param name="players">A list of player usernames who participated in the game.</param>
    /// <remarks>
    /// This method calculates pairwise compatibility scores for all unique player pairs using the
    /// <see cref="_calculator"/> service. It identifies the best-matching pair, saves the entire
    /// game result to the database as a <see cref="GameHistoryRecord"/>, and updates each user's
    /// statistics through the <see cref="_userService"/>.
    /// <para>
    /// If fewer than two players are provided, the method exits without saving any data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var players = new List&lt;string&gt; { "Alice", "Bob", "Charlie" };
    /// SaveGame("ROOM123", players);
    /// </code>
    /// </example>
    public void SaveGame(string roomCode, List<string> players) {
        var results = new List<CompatibilityScore>();

        for (int i = 0; i < players.Count; i++)
        for (int j = i + 1; j < players.Count; j++)
            results.Add(_calculator.Calculate(roomCode, players[i], players[j]));

        if (!results.Any()) return;

        var best = results.OrderByDescending(r => r.Percentage).First();

        var history = new GameHistoryRecord {
            RoomCode = roomCode,
            PlayedDate = DateTime.Now,
            TotalPlayers = players.Count,
            PlayerUsernames = JsonSerializer.Serialize(players),
            BestMatchPlayer = best.Player2,
            BestMatchPercentage = best.Percentage,
            ResultsJson = JsonSerializer.Serialize(results)
        };
        _context.GameHistory.Add(history);
        _context.SaveChanges();
        // update user stats
        foreach (var r in results) {
            _userService.UpdateUserStatistics(r.Player1, r.Percentage, false);
            _userService.UpdateUserStatistics(r.Player2, r.Percentage, false);
        }
    }
}