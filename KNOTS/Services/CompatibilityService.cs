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
        public CompatibilityService(UserService userService) 
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
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string jsonString = reader.ReadToEnd();
                        _statements = JsonSerializer.Deserialize<List<GameStatement>>(jsonString) ?? new List<GameStatement>();
                    }
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

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(jsonString);
                }
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
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string json = reader.ReadToEnd();
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

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                }
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

            return playerUsernames
                .All(player => roomSwipes.Count(s => s.PlayerUsername == player) >= totalStatements);
        }
        public CompatibilityScore CalculateCompatibility(string roomCode, string player1, string player2)
        {
            var player1Swipes = GetPlayerSwipes(roomCode, player1);
            var player2Swipes = GetPlayerSwipes(roomCode, player2);

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
                // Stream naudojimas saugojimui į JSON failą
                   using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            writer.Write(json);
                        }
                
                foreach (var result in allResults)
                {
                    // Check if player1 was best match for player2
                    var bestForPlayer2 = allResults
                        .Where(r => r.Player1 == result.Player2 || r.Player2 == result.Player2)
                        .OrderByDescending(r => r.Percentage)
                        .FirstOrDefault();
                    
                    bool player1WasBestMatch = bestForPlayer2.Player1 == result.Player1 || bestForPlayer2.Player2 == result.Player1;
                    
                    // Update player1 stats
                    _userService.UpdateUserStatistics(result.Player1, result.Percentage, player1WasBestMatch);
            
                    // Check if player2 was best match for player1
                    var bestForPlayer1 = allResults
                        .Where(r => r.Player1 == result.Player1 || r.Player2 == result.Player1)
                        .OrderByDescending(r => r.Percentage)
                        .FirstOrDefault();
                    
                    bool player2WasBestMatch = bestForPlayer1.Player1 == result.Player2 || bestForPlayer1.Player2 == result.Player2;
            
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
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<List<GameHistoryEntry>>(json) ?? new List<GameHistoryEntry>();
                    }
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
            
            var stats = new Dictionary<string, object>
            {
                ["TotalSwipes"] = roomSwipes.Count,  
                ["UniquePlayers"] = roomSwipes.Select(s => s.PlayerUsername).Distinct().Count(),  
                ["UniqueStatements"] = roomSwipes.Select(s => s.StatementId).Distinct().Count(),  
                ["RightSwipes"] = roomSwipes.Count(s => s.AgreeWithStatement),  
                ["LeftSwipes"] = roomSwipes.Count(s => !s.AgreeWithStatement)  
            };

            return stats;
        }

        
        public int GetStatisticValue(string roomCode, string statKey)
        {
            var stats = GetRoomStatistics(roomCode);
            
            if (stats.ContainsKey(statKey))
            {
                return (int)stats[statKey];  
            }
            
            return 0;
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