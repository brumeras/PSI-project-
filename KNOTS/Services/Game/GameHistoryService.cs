using KNOTS.Compability;
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
        private readonly GameHistoryRepository _historyRepository;
        private readonly CompatibilityCalculator _compatibilityCalculator;
        private readonly StatisticsService _statisticsService;
        private readonly UserService _userService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GameHistoryService"/> class.
        /// </summary>
        /// <param name="historyRepository">The repository used for storing game history entries.</param>
        /// <param name="compatibilityCalculator">The calculator used to compute player compatibility scores.</param>
        /// <param name="statisticsService">The service responsible for logging overall game statistics.</param>
        /// <param name="userService">The service responsible for updating player-specific statistics.</param>
        public GameHistoryService(
            GameHistoryRepository historyRepository,
            CompatibilityCalculator compatibilityCalculator,
            StatisticsService statisticsService,
            UserService userService)
        {
            _historyRepository = historyRepository;
            _compatibilityCalculator = compatibilityCalculator;
            _statisticsService = statisticsService;
            _userService = userService;
        }

        /// <summary>
        /// Saves a completed game to history, calculates compatibility results,
        /// logs statistics, and updates user records.
        /// </summary>
        /// <param name="roomCode">The unique code identifying the game room.</param>
        /// <param name="playerUsernames">A list of usernames of the players who participated.</param>
        public void SaveGame(string roomCode, List<string> playerUsernames) {
            try {
                Console.WriteLine($"[SaveGameToHistory] Saving game for room {roomCode}");
                _statisticsService.LogStatistics(roomCode);
                
                var allResults = _compatibilityCalculator.CalculateAll(roomCode, playerUsernames);
                if (!allResults.Any()) return;

                var bestMatch = allResults.First();
                
                var historyEntry = new GameHistoryEntry {
                    RoomCode = roomCode,
                    PlayedDate = DateTime.Now,
                    TotalPlayers = playerUsernames.Count,
                    BestMatchPlayer = bestMatch.Player2,
                    BestMatchPercentage = bestMatch.Percentage,
                    AllResults = allResults
                };

                _historyRepository.Save(historyEntry);
                UpdatePlayerStatistics(allResults);

                Console.WriteLine($"Game history saved for room {roomCode}");
            }
            catch (Exception ex) { Console.WriteLine($"Error saving game history: {ex.Message}"); }
        }

        /// <summary>
        /// Updates individual player statistics based on the results of a completed game.
        /// </summary>
        /// <param name="allResults">The list of all compatibility scores calculated for the game.</param>
        private void UpdatePlayerStatistics(List<CompatibilityScore> allResults) {
            foreach (var result in allResults) {
                var bestForPlayer2 = allResults
                    .Where(r => r.Player1 == result.Player2 || r.Player2 == result.Player2)
                    .OrderByDescending(r => r.Percentage)
                    .FirstOrDefault();
                
                bool player1WasBestMatch = bestForPlayer2.Player1 == result.Player1 || bestForPlayer2.Player2 == result.Player1;
                _userService.UpdateUserStatistics(result.Player1, result.Percentage, player1WasBestMatch);
        
                var bestForPlayer1 = allResults
                    .Where(r => r.Player1 == result.Player1 || r.Player2 == result.Player1)
                    .OrderByDescending(r => r.Percentage)
                    .FirstOrDefault();
                
                bool player2WasBestMatch = bestForPlayer1.Player1 == result.Player2 || bestForPlayer1.Player2 == result.Player2;
                _userService.UpdateUserStatistics(result.Player2, result.Percentage, player2WasBestMatch);
            }
        }
        
        /// <summary>
        /// Retrieves the full game history for a specific player.
        /// </summary>
        /// <param name="playerUsername">The username of the player.</param>
        /// <returns>
        /// A list of <see cref="GameHistoryEntry"/> objects sorted by most recent first.
        /// </returns>
        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) 
            => _historyRepository.GetPlayerHistory(playerUsername);
        
        /// <summary>
        /// Retrieves all recorded game history entries across all rooms and players.
        /// </summary>
        /// <returns>
        /// A list of <see cref="GameHistoryEntry"/> objects sorted by most recent first.
        /// </returns>
        public List<GameHistoryEntry> GetAllHistory() 
            => _historyRepository.GetAll();
    }