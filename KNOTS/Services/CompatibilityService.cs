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

    public class CompatibilityService
    {
        private static readonly ConcurrentDictionary<string, List<PlayerSwipe>> _roomSwipes = new();
        
        private readonly string _dataDirectory = "GameData";
        private readonly string _statementsDirectory = "GameStatements";
        private readonly string _defaultStatementsFile = "statements.json";
        private readonly string _swipesFile = "active_swipes.json";
        private readonly string _historyFile = "game_history.json";
        
        private List<GameStatement> _statements = new();
        private readonly UserService _userService;
        public CompatibilityService(UserService userService) // Update constructor
        {
            _userService = userService;
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_statementsDirectory);
    
            LoadStatementsFromFile();
            LoadActiveSwipesFromFile();
        }
        
        
        private void LoadStatementsFromFile()
        {
            try
            {
                string filePath = Path.Combine(_statementsDirectory, _defaultStatementsFile);

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    _statements = JsonSerializer.Deserialize<List<GameStatement>>(jsonString) ?? new List<GameStatement>();
                }
                else
                {
                    CreateDefaultStatements();
                    SaveStatementsToFile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Klaida skaitant teiginius: {ex.Message}");
                CreateDefaultStatements();
            }
        }

        private void SaveStatementsToFile()
        {
            try
            {
                string filePath = Path.Combine(_statementsDirectory, _defaultStatementsFile);
                
                string jsonString = JsonSerializer.Serialize(_statements, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Klaida saugant teiginius: {ex.Message}");
            }
        }

        private void CreateDefaultStatements()
        {
            _statements = new List<GameStatement>
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

        public List<GameStatement> GetAllStatements()
        {
            return _statements.ToList();
        }

        public List<GameStatement> GetRandomStatements(int count)
        {
            var random = new Random();
            return _statements.OrderBy(x => random.Next()).Take(Math.Min(count, _statements.Count)).ToList();
        }
        

        private void LoadActiveSwipesFromFile()
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, _swipesFile);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, List<PlayerSwipe>>>(json);
                    
                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            _roomSwipes[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Klaida skaitant swipes: {ex.Message}");
            }
        }

        private void SaveActiveSwipesToFile()
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, _swipesFile);
                var data = _roomSwipes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Klaida saugant swipes: {ex.Message}");
            }
        }

        public bool SaveSwipe(string roomCode, string playerUsername, string statementId, bool swipeRight)
        {
            var statement = _statements.FirstOrDefault(s => s.Id == statementId);
            if (statement.Id == null)
            {
                return false;
            }

            var playerSwipe = new PlayerSwipe(
                playerUsername,
                statementId,
                statement.Text,
                swipeRight
            );

            if (!_roomSwipes.ContainsKey(roomCode))
            {
                _roomSwipes[roomCode] = new List<PlayerSwipe>();
            }

            _roomSwipes[roomCode].RemoveAll(s => 
                s.PlayerUsername == playerUsername && s.StatementId == statementId);

            _roomSwipes[roomCode].Add(playerSwipe);
            
            SaveActiveSwipesToFile();
            
            // BOXING/UNBOXING USAGE - Log statistics kada išsaugomas swipe
            LogRoomStatistics(roomCode);
            
            return true;
        }

        public List<PlayerSwipe> GetRoomSwipes(string roomCode)
        {
            _roomSwipes.TryGetValue(roomCode, out var swipes);
            return swipes ?? new List<PlayerSwipe>();
        }

        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername)
        {
            var roomSwipes = GetRoomSwipes(roomCode);
            return roomSwipes.Where(s => s.PlayerUsername == playerUsername).ToList();
        }

        public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements)
        {
            var roomSwipes = GetRoomSwipes(roomCode);
            
            // BOXING/UNBOXING USAGE - Naudojame statistiką patikrinti progress
            var uniquePlayers = GetStatisticValue(roomCode, "UniquePlayers");
            var totalSwipes = GetStatisticValue(roomCode, "TotalSwipes");
            
            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: {uniquePlayers} players, {totalSwipes} total swipes");
            
            foreach (var player in playerUsernames)
            {
                var playerSwipeCount = roomSwipes.Count(s => s.PlayerUsername == player);
                if (playerSwipeCount < totalStatements)
                {
                    return false;
                }
            }
            
            return true;
        }

        public CompatibilityScore CalculateCompatibility(string roomCode, string player1, string player2)
        {
            var player1Swipes = GetPlayerSwipes(roomCode, player1);
            var player2Swipes = GetPlayerSwipes(roomCode, player2);

            int matchingSwipes = 0;
            var matchedStatements = new List<string>();

            foreach (var swipe1 in player1Swipes)
            {
                var swipe2 = player2Swipes.FirstOrDefault(s => s.StatementId == swipe1.StatementId);
                
                if (swipe2.StatementId != null && 
                    swipe1.AgreeWithStatement == swipe2.AgreeWithStatement)
                {
                    matchingSwipes++;
                    matchedStatements.Add(swipe1.StatementText);
                }
            }

            int totalStatements = Math.Min(player1Swipes.Count, player2Swipes.Count);

            return new CompatibilityScore(
                player1,
                player2,
                matchingSwipes,
                totalStatements,
                matchedStatements
            );
        }

        public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames)
        {
            var results = new List<CompatibilityScore>();

            // BOXING/UNBOXING USAGE - Log prieš skaičiuojant
            Console.WriteLine($"[CalculateAllCompatibilities] Starting calculation for room {roomCode}");
            LogRoomStatistics(roomCode);

            for (int i = 0; i < playerUsernames.Count; i++)
            {
                for (int j = i + 1; j < playerUsernames.Count; j++)
                {
                    var score = CalculateCompatibility(roomCode, playerUsernames[i], playerUsernames[j]);
                    results.Add(score);
                }
            }

            return results.OrderByDescending(r => r.Percentage).ToList();
        }

        public CompatibilityScore? GetBestMatch(string roomCode, List<string> playerUsernames)
        {
            var allScores = CalculateAllCompatibilities(roomCode, playerUsernames);
            return allScores.FirstOrDefault();
        }

        public CompatibilityScore? GetBestMatchForPlayer(string roomCode, string playerUsername, List<string> otherPlayers)
        {
            var scores = new List<CompatibilityScore>();

            foreach (var otherPlayer in otherPlayers)
            {
                if (otherPlayer != playerUsername)
                {
                    var score = CalculateCompatibility(roomCode, playerUsername, otherPlayer);
                    scores.Add(score);
                }
            }

            return scores.OrderByDescending(s => s.Percentage).FirstOrDefault();
        }
        
        public void SaveGameToHistory(string roomCode, List<string> playerUsernames)
        {
            try
            {
                // BOXING/UNBOXING USAGE - Log statistiką prieš išsaugant
                Console.WriteLine($"[SaveGameToHistory] Saving game for room {roomCode}");
                LogRoomStatistics(roomCode);
                
                var allResults = CalculateAllCompatibilities(roomCode, playerUsernames);
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

                var history = LoadGameHistory();
                history.Add(historyEntry);
                
                string filePath = Path.Combine(_dataDirectory, _historyFile);
                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(filePath, json);
                foreach (var result in allResults)
                {
                    // Check if player1 was best match for player2
                    bool player1WasBestMatch = allResults
                                                   .Where(r => r.Player1 == result.Player2 || r.Player2 == result.Player2)
                                                   .OrderByDescending(r => r.Percentage)
                                                   .First().Player1 == result.Player1 || 
                                               allResults
                                                   .Where(r => r.Player1 == result.Player2 || r.Player2 == result.Player2)
                                                   .OrderByDescending(r => r.Percentage)
                                                   .First().Player2 == result.Player1;
            
                    // Update player1 stats
                    _userService.UpdateUserStatistics(result.Player1, result.Percentage, player1WasBestMatch);
            
                    // Check if player2 was best match for player1
                    bool player2WasBestMatch = allResults
                                                   .Where(r => r.Player1 == result.Player1 || r.Player2 == result.Player1)
                                                   .OrderByDescending(r => r.Percentage)
                                                   .First().Player1 == result.Player2 || 
                                               allResults
                                                   .Where(r => r.Player1 == result.Player1 || r.Player2 == result.Player1)
                                                   .OrderByDescending(r => r.Percentage)
                                                   .First().Player2 == result.Player2;
            
                    // Update player2 stats
                    _userService.UpdateUserStatistics(result.Player2, result.Percentage, player2WasBestMatch);
                }

                Console.WriteLine($"Game history saved for room {roomCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game history: {ex.Message}");
            }
        }

        private List<GameHistoryEntry> LoadGameHistory()
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, _historyFile);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<GameHistoryEntry>>(json) ?? new List<GameHistoryEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game history: {ex.Message}");
            }
            return new List<GameHistoryEntry>();
        }

        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername)
        {
            var allHistory = LoadGameHistory();
            return allHistory
                .Where(h => h.AllResults.Any(r => r.Player1 == playerUsername || r.Player2 == playerUsername))
                .OrderByDescending(h => h.PlayedDate)
                .ToList();
        }

        public List<GameHistoryEntry> GetAllHistory()
        {
            return LoadGameHistory().OrderByDescending(h => h.PlayedDate).ToList();
        }

        public void ClearRoomData(string roomCode)
        {
            _roomSwipes.TryRemove(roomCode, out _);
            SaveActiveSwipesToFile();
        }
        
        // === BOXING/UNBOXING METHODS ===

        public Dictionary<string, object> GetRoomStatistics(string roomCode)
        {
            var roomSwipes = GetRoomSwipes(roomCode);
            
            // BOXING: value types (int, bool) -> object (reference type)
            var stats = new Dictionary<string, object>
            {
                ["TotalSwipes"] = roomSwipes.Count,  // int -> object (BOXING)
                ["UniquePlayers"] = roomSwipes.Select(s => s.PlayerUsername).Distinct().Count(),  // int -> object (BOXING)
                ["UniqueStatements"] = roomSwipes.Select(s => s.StatementId).Distinct().Count(),  // int -> object (BOXING)
                ["RightSwipes"] = roomSwipes.Count(s => s.AgreeWithStatement),  // int -> object (BOXING)
                ["LeftSwipes"] = roomSwipes.Count(s => !s.AgreeWithStatement)  // int -> object (BOXING)
            };

            return stats;
        }

        // UNBOXING: object -> int
        public int GetStatisticValue(string roomCode, string statKey)
        {
            var stats = GetRoomStatistics(roomCode);
            
            if (stats.ContainsKey(statKey))
            {
                return (int)stats[statKey];  // UNBOXING: object -> int (value type)
            }
            
            return 0;
        }

        public double GetPlayerProgressPercentage(string roomCode, string playerUsername, int totalStatements)
        {
            var swipes = GetPlayerSwipes(roomCode, playerUsername);
            
            if (totalStatements == 0) return 0.0;
            
            return Math.Round((double)swipes.Count / totalStatements * 100, 2);
        }
        
        // Helper metodas, kuris naudoja boxing/unboxing
        private void LogRoomStatistics(string roomCode)
        {
            // UNBOXING vyksta čia - gauname int iš Dictionary<string, object>
            var totalSwipes = GetStatisticValue(roomCode, "TotalSwipes");
            var uniquePlayers = GetStatisticValue(roomCode, "UniquePlayers");
            var rightSwipes = GetStatisticValue(roomCode, "RightSwipes");
            var leftSwipes = GetStatisticValue(roomCode, "LeftSwipes");
            
            Console.WriteLine($"[Room {roomCode}] Total: {totalSwipes}, Players: {uniquePlayers}, Right: {rightSwipes}, Left: {leftSwipes}");
        }
    }
    
}