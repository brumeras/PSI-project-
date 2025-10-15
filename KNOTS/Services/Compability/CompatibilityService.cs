using System.Collections.Concurrent;
using System.Text.Json;
using KNOTS.Compability;
using KNOTS.Services.Compability;

namespace KNOTS.Services {
    public class CompatibilityService
    {
        private readonly StatementRepository _statementRepository;
        private readonly SwipeRepository _swipeRepository;
        private readonly CompatibilityCalculator _compatibilityCalculator;
        private readonly StatisticsService _statisticsService;
        private readonly GameProgressChecker _progressChecker;
        private readonly GameHistoryService _historyService;

        public CompatibilityService(UserService userService) 
            : this(userService, "GameData", "GameStatements")
        {
        }

        public CompatibilityService(UserService userService, string dataDirectory, string statementsDirectory)
        {
            // Initialize file repositories
            var statementFileRepo = new JsonFileRepository<List<GameStatement>>(statementsDirectory, "statements.json");
            var swipeFileRepo = new JsonFileRepository<Dictionary<string, List<PlayerSwipe>>>(dataDirectory, "active_swipes.json");
            var historyFileRepo = new JsonFileRepository<List<GameHistoryEntry>>(dataDirectory, "game_history.json");

            // Initialize domain repositories
            _statementRepository = new StatementRepository(statementFileRepo);
            _swipeRepository = new SwipeRepository(swipeFileRepo);
            var historyRepository = new GameHistoryRepository(historyFileRepo);

            // Initialize services
            _compatibilityCalculator = new CompatibilityCalculator(_swipeRepository);
            _statisticsService = new StatisticsService(_swipeRepository);
            _progressChecker = new GameProgressChecker(_swipeRepository, _statisticsService);
            _historyService = new GameHistoryService(historyRepository, _compatibilityCalculator, _statisticsService, userService);
        }

        // Public API methods
        public List<GameStatement> GetRandomStatements(int count) 
            => _statementRepository.GetRandom(count);

        public bool SaveSwipe(string roomCode, string playerUsername, string statementId, bool swipeRight)
        {
            var statement = _statementRepository.GetById(statementId);
            if (statement == null) return false;

            var playerSwipe = new PlayerSwipe(playerUsername, statementId, statement.Value.Text, swipeRight);
            var result = _swipeRepository.SaveSwipe(roomCode, playerSwipe);
            
            if (result)
            {
                _statisticsService.LogStatistics(roomCode);
            }
            
            return result;
        }

        public List<PlayerSwipe> GetRoomSwipes(string roomCode) 
            => _swipeRepository.GetRoomSwipes(roomCode);

        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) 
            => _swipeRepository.GetPlayerSwipes(roomCode, playerUsername);

        public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements) 
            => _progressChecker.HaveAllPlayersFinished(roomCode, playerUsernames, totalStatements);

        public CompatibilityScore CalculateCompatibility(string roomCode, string player1, string player2) 
            => _compatibilityCalculator.Calculate(roomCode, player1, player2);

        public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames)
        {
            Console.WriteLine($"[CalculateAllCompatibilities] Starting calculation for room {roomCode}");
            _statisticsService.LogStatistics(roomCode);
            return _compatibilityCalculator.CalculateAll(roomCode, playerUsernames);
        }

        public CompatibilityScore? GetBestMatch(string roomCode, List<string> playerUsernames) 
            => _compatibilityCalculator.GetBestMatch(roomCode, playerUsernames);

        public void SaveGameToHistory(string roomCode, List<string> playerUsernames) 
            => _historyService.SaveGame(roomCode, playerUsernames);

        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) 
            => _historyService.GetPlayerHistory(playerUsername);

        public List<GameHistoryEntry> GetAllHistory() 
            => _historyService.GetAllHistory();

        public void ClearRoomData(string roomCode) 
            => _swipeRepository.ClearRoomData(roomCode);

        public RoomStatistics GetRoomStatistics(string roomCode) 
            => _statisticsService.GetRoomStatistics(roomCode);
    }
}