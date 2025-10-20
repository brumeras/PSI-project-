using System.Collections.Concurrent;
using System.Text.Json;

namespace KNOTS.Services
{
 
    public struct GameStatement
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public GameStatement(string id, string text)
        {
            Id = id;
            Text = text;
        }
    }
    
    public struct PlayerSwipe
    {
        public string PlayerUsername { get; set; }
        public string StatementId { get; set; }
        public string StatementText { get; set; }
        public bool AgreeWithStatement { get; set; }
        public DateTime SwipedAt { get; set; }

        public PlayerSwipe(string playerUsername, string statementId, string statementText, bool agreeWithStatement)
        {
            PlayerUsername = playerUsername;
            StatementId = statementId;
            StatementText = statementText;
            AgreeWithStatement = agreeWithStatement;
            SwipedAt = DateTime.Now;
        }
    }
    
    public struct CompatibilityScore
    {
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public int MatchingSwipes { get; set; }
        public int TotalStatements { get; set; }
        public List<string> MatchedStatements { get; set; }

        public CompatibilityScore(string player1, string player2, int matchingSwipes, int totalStatements, List<string> matchedStatements)
        {
            Player1 = player1;
            Player2 = player2;
            MatchingSwipes = matchingSwipes;
            TotalStatements = totalStatements;
            MatchedStatements = matchedStatements;
        }

        public double Percentage => TotalStatements > 0 
            ? Math.Round((double)MatchingSwipes / TotalStatements * 100, 2)
            : 0;
    }
    
    public class GameHistoryEntry
    {
        public string RoomCode { get; set; } = "";
        public DateTime PlayedDate { get; set; }
        public int TotalPlayers { get; set; }
        public string BestMatchPlayer { get; set; } = "";
        public double BestMatchPercentage { get; set; }
        public List<CompatibilityScore> AllResults { get; set; } = new();
    }

    public class RoomStatistics
    {
        public int TotalSwipes { get; set; }
        public int UniquePlayers { get; set; }
        public int UniqueStatements { get; set; }
        public int RightSwipes { get; set; }
        public int LeftSwipes { get; set; }
    }

    // ===== FILE REPOSITORY  =====
    
    public class JsonFileRepository<T> where T : new()
    {
        private readonly string _filePath;

        public JsonFileRepository(string directory, string fileName)
        {
            Directory.CreateDirectory(directory);
            _filePath = Path.Combine(directory, fileName);
        }

        public T Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(fs);
                    string json = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<T>(json) ?? new T();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading from {_filePath}: {ex.Message}");
            }

            return new T();
        }

        public void Save(T data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(fs);
                writer.Write(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to {_filePath}: {ex.Message}");
            }
        }
    }

    // ===== STATEMENT REPOSITORY  =====
    
    public class StatementRepository
    {
        private readonly JsonFileRepository<List<GameStatement>> _fileRepository;
        private List<GameStatement> _statements;

        public StatementRepository(JsonFileRepository<List<GameStatement>> fileRepository)
        {
            _fileRepository = fileRepository;
            _statements = _fileRepository.Load();
            
            if (!_statements.Any())
            {
                _statements = CreateDefaultStatements();
                _fileRepository.Save(_statements);
            }
        }

        public List<GameStatement> GetAll() => _statements;

        public List<GameStatement> GetRandom(int count)
        {
            var random = new Random();
            return _statements.OrderBy(x => random.Next()).Take(Math.Min(count, _statements.Count)).ToList();
        }

        public GameStatement? GetById(string id)
        {
            var statement = _statements.FirstOrDefault(s => s.Id == id);
            return statement.Id != null ? statement : null;
        }

        private List<GameStatement> CreateDefaultStatements()
        {
            return new List<GameStatement>
            {
                new GameStatement("S1", "I like getting up early in the morning"),
                new GameStatement("S2", "I prefer relaxing at home over going to parties"),
                new GameStatement("S3", "I enjoy spontaneous trips"),
                new GameStatement("S4", "Animals are an important part of my life"),
                new GameStatement("S5", "I prefer movies over theater"),
                new GameStatement("S6", "Sports are part of my daily routine"),
                new GameStatement("S7", "I enjoy cooking at home"),
                new GameStatement("S8", "Summer is the best season"),
                new GameStatement("S9", "Meaningful conversations matter more to me than having fun"),
                new GameStatement("S10", "I like taking risks and trying new things"),
                new GameStatement("S11", "Music is an important part of my life"),
                new GameStatement("S12", "I value personal space in relationships"),
                new GameStatement("S13", "I like to plan everything in advance"),
                new GameStatement("S14", "I feel good at large parties"),
                new GameStatement("S15", "I live in the moment and don't worry about the future"),
                new GameStatement("S16", "Romantic relationships are important to me"),
                new GameStatement("S17", "I like video games"),
                new GameStatement("S18", "Books are better than movies"),
                new GameStatement("S19", "I enjoy nature and hiking"),
                new GameStatement("S20", "Financial stability is a priority"),
            };
        }
    }

    // ===== SWIPE REPOSITORY  =====
    
    public class SwipeRepository
    {
        private readonly ConcurrentDictionary<string, List<PlayerSwipe>> _roomSwipes = new();
        private readonly JsonFileRepository<Dictionary<string, List<PlayerSwipe>>> _fileRepository;
        private readonly object _lockObject = new object();

        public SwipeRepository(JsonFileRepository<Dictionary<string, List<PlayerSwipe>>> fileRepository)
        {
            _fileRepository = fileRepository;
            LoadSwipes();
        }

        private void LoadSwipes()
        {
            lock (_lockObject)
            {
                var data = _fileRepository.Load();
                _roomSwipes.Clear();
                foreach (var kvp in data)
                {
                    _roomSwipes[kvp.Key] = kvp.Value;
                }
            }
        }

        public bool SaveSwipe(string roomCode, PlayerSwipe swipe)
        {
            lock (_lockObject)
            {
                
                LoadSwipesInternal();

                if (!_roomSwipes.ContainsKey(roomCode))
                {
                    _roomSwipes[roomCode] = new List<PlayerSwipe>();
                }

                _roomSwipes[roomCode].RemoveAll(s => 
                    s.PlayerUsername == swipe.PlayerUsername && s.StatementId == swipe.StatementId);

                _roomSwipes[roomCode].Add(swipe);
                
                PersistSwipes();
                
                Console.WriteLine($"[SwipeRepository] Saved swipe for {swipe.PlayerUsername} in room {roomCode}. Total swipes in room: {_roomSwipes[roomCode].Count}");
                return true;
            }
        }

        public List<PlayerSwipe> GetRoomSwipes(string roomCode)
        {
            lock (_lockObject)
            {
                // Reload to get fresh data
                LoadSwipesInternal();
                
                _roomSwipes.TryGetValue(roomCode, out var swipes);
                var result = swipes ?? new List<PlayerSwipe>();
                
                Console.WriteLine($"[SwipeRepository] GetRoomSwipes for {roomCode}: Found {result.Count} swipes");
                return result;
            }
        }

        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername)
        {
            var roomSwipes = GetRoomSwipes(roomCode);
            var playerSwipes = roomSwipes.Where(s => s.PlayerUsername == playerUsername).ToList();
            
            Console.WriteLine($"[SwipeRepository] GetPlayerSwipes for {playerUsername} in room {roomCode}: Found {playerSwipes.Count} swipes");
            return playerSwipes;
        }

        public void ClearRoomData(string roomCode)
        {
            lock (_lockObject)
            {
                _roomSwipes.TryRemove(roomCode, out _);
                PersistSwipes();
            }
        }

        private void LoadSwipesInternal()
        {
            var data = _fileRepository.Load();
            _roomSwipes.Clear();
            foreach (var kvp in data)
            {
                _roomSwipes[kvp.Key] = kvp.Value;
            }
        }

        private void PersistSwipes()
        {
            var data = _roomSwipes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _fileRepository.Save(data);
        }
    }

    // ===== GAME HISTORY REPOSITORY  =====
    
    public class GameHistoryRepository
    {
        private readonly JsonFileRepository<List<GameHistoryEntry>> _fileRepository;

        public GameHistoryRepository(JsonFileRepository<List<GameHistoryEntry>> fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public void Save(GameHistoryEntry entry)
        {
            var history = _fileRepository.Load();
            history.Add(entry);
            _fileRepository.Save(history);
        }

        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername)
        {
            var allHistory = _fileRepository.Load();
            return allHistory
                .Where(h => h.AllResults.Any(r => r.Player1 == playerUsername || r.Player2 == playerUsername))
                .OrderByDescending(h => h.PlayedDate)
                .ToList();
        }

        public List<GameHistoryEntry> GetAll()
        {
            return _fileRepository.Load().OrderByDescending(h => h.PlayedDate).ToList();
        }
    }

    // ===== COMPATIBILITY CALCULATOR  =====
    
    public class CompatibilityCalculator
    {
        private readonly SwipeRepository _swipeRepository;

        public CompatibilityCalculator(SwipeRepository swipeRepository)
        {
            _swipeRepository = swipeRepository;
        }

        public CompatibilityScore Calculate(string roomCode, string player1, string player2)
        {
            var player1Swipes = _swipeRepository.GetPlayerSwipes(roomCode, player1);
            var player2Swipes = _swipeRepository.GetPlayerSwipes(roomCode, player2);

            var matchedStatements = player1Swipes
                .Join(player2Swipes,
                    s1 => s1.StatementId,
                    s2 => s2.StatementId,
                    (s1, s2) => new { s1, s2 })
                .Where(pair => pair.s1.AgreeWithStatement == pair.s2.AgreeWithStatement)
                .Select(pair => pair.s1.StatementText)
                .ToList();
            
            int matchingSwipes = matchedStatements.Count;
            int totalStatements = Math.Min(player1Swipes.Count, player2Swipes.Count);

            return new CompatibilityScore(
                player1,
                player2,
                matchingSwipes,
                totalStatements,
                matchedStatements
            );
        }

        public List<CompatibilityScore> CalculateAll(string roomCode, List<string> playerUsernames)
        {
            var results = new List<CompatibilityScore>();

            for (int i = 0; i < playerUsernames.Count; i++)
            {
                for (int j = i + 1; j < playerUsernames.Count; j++)
                {
                    var score = Calculate(roomCode, playerUsernames[i], playerUsernames[j]);
                    results.Add(score);
                }
            }

            return results.OrderByDescending(r => r.Percentage).ToList();
        }

        public CompatibilityScore? GetBestMatch(string roomCode, List<string> playerUsernames)
        {
            var allScores = CalculateAll(roomCode, playerUsernames);
            return allScores.FirstOrDefault();
        }
    }

    // ===== STATISTICS SERVICE =====
    
    public class StatisticsService
    {
        private readonly SwipeRepository _swipeRepository;

        public StatisticsService(SwipeRepository swipeRepository)
        {
            _swipeRepository = swipeRepository;
        }

        public RoomStatistics GetRoomStatistics(string roomCode)
        {
            var roomSwipes = _swipeRepository.GetRoomSwipes(roomCode);
            
            return new RoomStatistics
            {
                TotalSwipes = roomSwipes.Count,
                UniquePlayers = roomSwipes.Select(s => s.PlayerUsername).Distinct().Count(),
                UniqueStatements = roomSwipes.Select(s => s.StatementId).Distinct().Count(),
                RightSwipes = roomSwipes.Count(s => s.AgreeWithStatement),
                LeftSwipes = roomSwipes.Count(s => !s.AgreeWithStatement)
            };
        }

        public void LogStatistics(string roomCode)
        {
            var stats = GetRoomStatistics(roomCode);
            Console.WriteLine($"[Room {roomCode}] Total: {stats.TotalSwipes}, Players: {stats.UniquePlayers}, Right: {stats.RightSwipes}, Left: {stats.LeftSwipes}");
        }
    }

    // ===== GAME PROGRESS CHECKER  =====
    
    public class GameProgressChecker
    {
        private readonly SwipeRepository _swipeRepository;
        private readonly StatisticsService _statisticsService;

        public GameProgressChecker(SwipeRepository swipeRepository, StatisticsService statisticsService)
        {
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

            var roomSwipes = _swipeRepository.GetRoomSwipes(roomCode);
            var stats = _statisticsService.GetRoomStatistics(roomCode);
            
            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: {stats.UniquePlayers} players, {stats.TotalSwipes} total swipes, Expected: {totalStatements} statements per player");

            foreach (var player in playerUsernames)
            {
                var playerSwipeCount = roomSwipes.Count(s => s.PlayerUsername == player);
                Console.WriteLine($"[HaveAllPlayersFinished] Player {player}: {playerSwipeCount}/{totalStatements} swipes");
                
                if (playerSwipeCount < totalStatements)
                {
                    return false;
                }
            }

            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: All players finished!");
            return true;
        }
    }

    // ===== GAME HISTORY SERVICE  =====
    
    public class GameHistoryService
    {
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

        public void SaveGame(string roomCode, List<string> playerUsernames)
        {
            try
            {
                Console.WriteLine($"[SaveGameToHistory] Saving game for room {roomCode}");
                _statisticsService.LogStatistics(roomCode);
                
                var allResults = _compatibilityCalculator.CalculateAll(roomCode, playerUsernames);
                if (!allResults.Any()) return;

                var bestMatch = allResults.First();
                
                var historyEntry = new GameHistoryEntry
                {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game history: {ex.Message}");
            }
        }

        private void UpdatePlayerStatistics(List<CompatibilityScore> allResults)
        {
            foreach (var result in allResults)
            {
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
        
    }
}