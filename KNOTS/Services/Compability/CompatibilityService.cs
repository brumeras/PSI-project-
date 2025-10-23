using System.Collections.Concurrent;
using System.Text.Json;
using KNOTS.Compability;
using KNOTS.Services.Compability;

namespace KNOTS.Services {
    
    /// <summary>
    /// Provides operations for handling player swipes, calculating compatibility,
    /// tracking game progress, and managing game history.
    /// </summary>
    /// <remarks>
    /// This service acts as the main coordination layer between repositories and domain services,
    /// such as <see cref="CompatibilityCalculator"/>, <see cref="StatisticsService"/>, 
    /// and <see cref="GameHistoryService"/>.
    /// </remarks>
    public class CompatibilityService
    {
        private readonly StatementRepository _statementRepository;
        private readonly SwipeRepository _swipeRepository;
        private readonly CompatibilityCalculator _compatibilityCalculator;
        private readonly StatisticsService _statisticsService;
        private readonly GameProgressChecker _progressChecker;
        private readonly GameHistoryService _historyService;

        /// <summary>
        /// Initializes a new <see cref="CompatibilityService"/> with default data.
        /// </summary>
        public CompatibilityService(UserService userService) 
            : this(userService, "GameData", "GameStatements")
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CompatibilityService"/> with custom repository paths.
        /// </summary>
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
        /// <summary>
        /// Retrieves a random subset of game statements.
        /// </summary>
        public List<GameStatement> GetRandomStatements(int count) 
            => _statementRepository.GetRandom(count);

        
        /// <summary>
        /// Saves a player's swipe (like/dislike) for a specific statement.
        /// </summary>
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

        
        /// <summary>
        /// Retrieves all swipes made within a room.
        /// </summary>
        public List<PlayerSwipe> GetRoomSwipes(string roomCode) 
            => _swipeRepository.GetRoomSwipes(roomCode);

        /// <summary>
        /// Retrieves all swipes made by a specific player in a room.
        /// </summary>
        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) 
            => _swipeRepository.GetPlayerSwipes(roomCode, playerUsername);

        /// <summary>
        /// Checks whether all players have completed swiping.
        /// </summary>
        public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements) 
            => _progressChecker.HaveAllPlayersFinished(roomCode, playerUsernames, totalStatements);

        /// <summary>
        /// Calculates compatibility between two specific players.
        /// </summary>
        public CompatibilityScore CalculateCompatibility(string roomCode, string player1, string player2) 
            => _compatibilityCalculator.Calculate(roomCode, player1, player2);

        /// <summary>
        /// Calculates compatibility scores for all player in a room.
        /// </summary>
        public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames)
        {
            Console.WriteLine($"[CalculateAllCompatibilities] Starting calculation for room {roomCode}");
            _statisticsService.LogStatistics(roomCode);
            return _compatibilityCalculator.CalculateAll(roomCode, playerUsernames);
        }

        /// <summary>
        /// Finds the best matching pair of players in a room.
        /// </summary>
        public CompatibilityScore? GetBestMatch(string roomCode, List<string> playerUsernames) 
            => _compatibilityCalculator.GetBestMatch(roomCode, playerUsernames);

        /// <summary>
        /// Saves the completed game data to the history repository.
        /// </summary>
        public void SaveGameToHistory(string roomCode, List<string> playerUsernames) 
            => _historyService.SaveGame(roomCode, playerUsernames);

        /// <summary>
        /// Retrieves all game history for a specific player.
        /// </summary>
        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) 
            => _historyService.GetPlayerHistory(playerUsername);

        /// <summary>
        /// Retrieves the complete game history.
        /// </summary>
        public List<GameHistoryEntry> GetAllHistory() 
            => _historyService.GetAllHistory();

        /// <summary>
        /// Clears all swipe data for a specific room.
        /// </summary>
        public void ClearRoomData(string roomCode) 
            => _swipeRepository.ClearRoomData(roomCode);

        /// <summary>
        /// Retrieves room statistics such as swipe counts and matches.
        /// </summary>
        public RoomStatistics GetRoomStatistics(string roomCode) 
            => _statisticsService.GetRoomStatistics(roomCode);
    }
}