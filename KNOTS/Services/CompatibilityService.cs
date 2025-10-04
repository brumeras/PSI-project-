using System.Collections.Concurrent;
using System.Text.Json;

namespace KNOTS.Services
{
    // Struct klausimui/teiginniui
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

    // Struct žaidėjo swipe'ui
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

    // Struct compatibility rezultatui
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

    // Klasė istorijos saugojimui
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
        // In-memory storage (aktyvūs žaidimai)
        private readonly ConcurrentDictionary<string, List<PlayerSwipe>> _roomSwipes = new();
        
        // Failų keliai
        private readonly string _dataDirectory = "GameData";
        private readonly string _statementsDirectory = "GameStatements";
        private readonly string _defaultStatementsFile = "statements.json";
        private readonly string _swipesFile = "active_swipes.json";
        private readonly string _historyFile = "game_history.json";
        
        private List<GameStatement> _statements = new();

        public CompatibilityService()
        {
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_statementsDirectory);
            
            LoadStatementsFromFile();
            LoadActiveSwipesFromFile();
        }

        // === STATEMENTS MANAGEMENT ===
        
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
                new GameStatement("S1", "Mėgstu anksti keltis rytais"),
                new GameStatement("S2", "Pirmenybę teikiu namų poilsiui nei vakarėliams"),
                new GameStatement("S3", "Mėgstu spontaniškas keliones"),
                new GameStatement("S4", "Gyvūnai yra svarbi mano gyvenimo dalis"),
                new GameStatement("S5", "Geriau kinas nei teatras"),
                new GameStatement("S6", "Sportas yra mano kasdienybės dalis"),
                new GameStatement("S7", "Mėgstu gaminti namuose"),
                new GameStatement("S8", "Vasara yra geriausia metų laikas"),
                new GameStatement("S9", "Protingi pokalbiai yra svarbesni už linksmybes"),
                new GameStatement("S10", "Mėgstu rizikuoti ir išbandyti naujus dalykus"),
                new GameStatement("S11", "Muzika yra svarbi mano gyvenime"),
                new GameStatement("S12", "Vertinu asmeninę erdvę santykiuose"),
                new GameStatement("S13", "Mėgstu planuoti viską iš anksto"),
                new GameStatement("S14", "Dideliuose vakarėliuose jaučiuosi gerai"),
                new GameStatement("S15", "Gyvenu čia ir dabar, nerūpinausi ateitimi"),
                new GameStatement("S16", "Romantiškos pažintys man svarbios"),
                new GameStatement("S17", "Mėgstu video žaidimus"),
                new GameStatement("S18", "Knygos geriau nei filmai"),
                new GameStatement("S19", "Mėgstu gamtą ir žygius"),
                new GameStatement("S20", "Finansinis stabilumas yra prioritetas")
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

        // === ACTIVE SWIPES (JSON) ===

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

        // === COMPATIBILITY CALCULATION ===

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

        // === GAME HISTORY (JSON) ===

        public void SaveGameToHistory(string roomCode, List<string> playerUsernames)
        {
            try
            {
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

                // Load existing history
                var history = LoadGameHistory();
                history.Add(historyEntry);

                // Save updated history
                string filePath = Path.Combine(_dataDirectory, _historyFile);
                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(filePath, json);

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

        // === BOXING/UNBOXING EXAMPLES ===

        public Dictionary<string, object> GetRoomStatistics(string roomCode)
        {
            var roomSwipes = GetRoomSwipes(roomCode);
            
            // BOXING: value types -> object
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

        // UNBOXING
        public int GetStatisticValue(string roomCode, string statKey)
        {
            var stats = GetRoomStatistics(roomCode);
            
            if (stats.ContainsKey(statKey))
            {
                return (int)stats[statKey];  // Unboxing: object -> int
            }
            
            return 0;
        }

        public double GetPlayerProgressPercentage(string roomCode, string playerUsername, int totalStatements)
        {
            var swipes = GetPlayerSwipes(roomCode, playerUsername);
            
            if (totalStatements == 0) return 0.0;
            
            // BOXING happens when returning
            return Math.Round((double)swipes.Count / totalStatements * 100, 2);
        }
    }
}