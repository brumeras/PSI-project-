using KNOTS.Compability;
using KNOTS.Services.Compability;

namespace KNOTS.Services;

public class GameHistoryService {
        private readonly GameHistoryRepository _historyRepository;
        private readonly CompatibilityCalculator _compatibilityCalculator;
        private readonly StatisticsService _statisticsService;
        private readonly UserService _userService;
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
        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) 
            => _historyRepository.GetPlayerHistory(playerUsername);
        public List<GameHistoryEntry> GetAllHistory() 
            => _historyRepository.GetAll();
    }