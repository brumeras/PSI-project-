using System.Text.Json;
using KNOTS.Compability;
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;


public class CompatibilityService {
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        public CompatibilityService(AppDbContext context, UserService userService) {
            _context = context;
            _userService = userService;
            
            Console.WriteLine("🔧 CompatibilityService created with DB context");
            EnsureDefaultStatements();
        }
        private void EnsureDefaultStatements()
        {
            if (!_context.Statements.Any())
            {
                Console.WriteLine("📝 Creating default statements in database...");

                var defaultStatements = new List<GameStatement>
                {
                    new GameStatement { Id = "S1", Text = "I like getting up early in the morning" },
                    new GameStatement { Id = "S2", Text = "I prefer relaxing at home over going to parties" },
                    new GameStatement { Id = "S3", Text = "I enjoy spontaneous trips" },
                    new GameStatement { Id = "S4", Text = "Animals are an important part of my life" },
                    new GameStatement { Id = "S5", Text = "I prefer movies over theater" },
                    new GameStatement { Id = "S6", Text = "Sports are part of my daily routine" },
                    new GameStatement { Id = "S7", Text = "I enjoy cooking at home" },
                    new GameStatement { Id = "S8", Text = "Summer is the best season" },
                    new GameStatement
                        { Id = "S9", Text = "Meaningful conversations matter more to me than having fun" },
                    new GameStatement { Id = "S10", Text = "I like taking risks and trying new things" },
                    new GameStatement { Id = "S11", Text = "Music is an important part of my life" },
                    new GameStatement { Id = "S12", Text = "I value personal space in relationships" },
                    new GameStatement { Id = "S13", Text = "I like to plan everything in advance" },
                    new GameStatement { Id = "S14", Text = "I feel good at large parties" },
                    new GameStatement { Id = "S15", Text = "I live in the moment and don't worry about the future" },
                    new GameStatement { Id = "S16", Text = "Romantic relationships are important to me" },
                    new GameStatement { Id = "S17", Text = "I like video games" },
                    new GameStatement { Id = "S18", Text = "Books are better than movies" },
                    new GameStatement { Id = "S19", Text = "I enjoy nature and hiking" },
                    new GameStatement { Id = "S20", Text = "Financial stability is a priority" }
                };

                _context.Statements.AddRange(defaultStatements);
                _context.SaveChanges();
                Console.WriteLine($"✅ Created {defaultStatements.Count} statements in database");
            }
            else { Console.WriteLine($"✅ Statements already exist in database ({_context.Statements.Count()} total)"); }
        }
        public List<GameStatement> GetRandomStatements(int count)
        {
            var random = new Random();
            return _context.Statements
                .AsEnumerable()
                .OrderBy(x => random.Next())
                .Take(Math.Min(count, _context.Statements.Count()))
                .ToList();
        }
        public bool SaveSwipe(string roomCode, string playerUsername, string statementId, bool swipeRight) {
            try {
                var statement = _context.Statements.FirstOrDefault(s => s.Id == statementId);
                if (statement == null) {
                    Console.WriteLine($"❌ Statement {statementId} not found");
                    return false;
                }

                var existing = _context.PlayerSwipes
                    .FirstOrDefault(s => s.RoomCode == roomCode && 
                                       s.PlayerUsername == playerUsername && 
                                       s.StatementId == statementId);
                
                if (existing != null) { _context.PlayerSwipes.Remove(existing); }

                var swipeRecord = new PlayerSwipeRecord {
                    RoomCode = roomCode,
                    PlayerUsername = playerUsername,
                    StatementId = statementId,
                    StatementText = statement.Text,
                    AgreeWithStatement = swipeRight,
                    SwipedAt = DateTime.Now
                };

                _context.PlayerSwipes.Add(swipeRecord);
                _context.SaveChanges();
                
                Console.WriteLine($"✅ Saved swipe: {playerUsername} {(swipeRight ? "agreed" : "disagreed")} with {statementId}");
                LogRoomStatistics(roomCode);
                
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine($"❌ Error saving swipe: {ex.Message}");
                return false;
            }
        }
        public List<PlayerSwipe> GetRoomSwipes(string roomCode) {
            return _context.PlayerSwipes
                .Where(s => s.RoomCode == roomCode)
                .Select(s => new PlayerSwipe(
                    s.PlayerUsername,
                    s.StatementId,
                    s.StatementText,
                    s.AgreeWithStatement
                ) { SwipedAt = s.SwipedAt })
                .ToList();
        }
        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) {
            return _context.PlayerSwipes
                .Where(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername)
                .Select(s => new PlayerSwipe(
                    s.PlayerUsername,
                    s.StatementId,
                    s.StatementText,
                    s.AgreeWithStatement
                ) { SwipedAt = s.SwipedAt })
                .ToList();
        }
        public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements) {
            var uniquePlayers = GetStatisticValue(roomCode, "UniquePlayers");
            var totalSwipes = GetStatisticValue(roomCode, "TotalSwipes");
            
            Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: {uniquePlayers} players, {totalSwipes} total swipes");

            foreach (var player in playerUsernames) {
                var swipeCount = _context.PlayerSwipes
                    .Count(s => s.RoomCode == roomCode && s.PlayerUsername == player);
                if (swipeCount < totalStatements) {return false; }
            }

            return true;
        }
        public CompatibilityScore CalculateCompatibility(string roomCode, string player1, string player2) {
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
        public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames) {
            var results = new List<CompatibilityScore>();

            Console.WriteLine($"[CalculateAllCompatibilities] Starting calculation for room {roomCode}");
            LogRoomStatistics(roomCode);

            for (int i = 0; i < playerUsernames.Count; i++) {
                for (int j = i + 1; j < playerUsernames.Count; j++) {
                    var score = CalculateCompatibility(roomCode, playerUsernames[i], playerUsernames[j]);
                    results.Add(score);
                }
            }
            return results.OrderByDescending(r => r.Percentage).ToList();
        }
        public CompatibilityScore? GetBestMatch(string roomCode, List<string> playerUsernames) {
            var allScores = CalculateAllCompatibilities(roomCode, playerUsernames);
            return allScores.FirstOrDefault();
        }
        public void SaveGameToHistory(string roomCode, List<string> playerUsernames) {
            try {
                Console.WriteLine($"[SaveGameToHistory] Saving game for room {roomCode}");
                LogRoomStatistics(roomCode);
                
                var allResults = CalculateAllCompatibilities(roomCode, playerUsernames);
                if (!allResults.Any()) return;

                var bestMatch = allResults.First();
                
                var historyRecord = new GameHistoryRecord {
                    RoomCode = roomCode,
                    PlayedDate = DateTime.Now,
                    TotalPlayers = playerUsernames.Count,
                    PlayerUsernames = JsonSerializer.Serialize(playerUsernames),
                    BestMatchPlayer = bestMatch.Player2,
                    BestMatchPercentage = bestMatch.Percentage,
                    ResultsJson = JsonSerializer.Serialize(allResults)
                };

                _context.GameHistory.Add(historyRecord);
                _context.SaveChanges();
                
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
                Console.WriteLine($"✅ Game history saved for room {roomCode}");
            }
            catch (Exception ex) { Console.WriteLine($"❌ Error saving game history: {ex.Message}"); }
        }
        public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) {
            // Get all history first, then filter in memory
            var allHistory = _context.GameHistory
                .OrderByDescending(h => h.PlayedDate)
                .ToList();
            
            return allHistory
                .Where(h => 
                {
                    var players = JsonSerializer.Deserialize<List<string>>(h.PlayerUsernames) ?? new List<string>();
                    return players.Contains(playerUsername);
                })
                .Select(h => new GameHistoryEntry
                {
                    RoomCode = h.RoomCode,
                    PlayedDate = h.PlayedDate,
                    TotalPlayers = h.TotalPlayers,
                    BestMatchPlayer = h.BestMatchPlayer,
                    BestMatchPercentage = h.BestMatchPercentage,
                    AllResults = JsonSerializer.Deserialize<List<CompatibilityScore>>(h.ResultsJson) ?? new List<CompatibilityScore>()
                })
                .ToList();
        }
        public List<GameHistoryEntry> GetAllHistory() {
            return _context.GameHistory
                .OrderByDescending(h => h.PlayedDate)
                .Select(h => new GameHistoryEntry
                {
                    RoomCode = h.RoomCode,
                    PlayedDate = h.PlayedDate,
                    TotalPlayers = h.TotalPlayers,
                    BestMatchPlayer = h.BestMatchPlayer,
                    BestMatchPercentage = h.BestMatchPercentage,
                    //AllResults = JsonSerializer.Deserialize<List<CompatibilityScore>>(h.ResultsJson) ?? new List<CompatibilityScore>()
                })
                .ToList();
        }
        public void ClearRoomData(string roomCode) {
            var swipesToRemove = _context.PlayerSwipes.Where(s => s.RoomCode == roomCode);
            _context.PlayerSwipes.RemoveRange(swipesToRemove);
            _context.SaveChanges();
            Console.WriteLine($"✅ Cleared data for room {roomCode}");
        }
    
        // === BOXING/UNBOXING METHODS ===
        public Dictionary<string, object> GetRoomStatistics(string roomCode) {
            var roomSwipes = _context.PlayerSwipes.Where(s => s.RoomCode == roomCode);
            
            var stats = new Dictionary<string, object> {
                ["TotalSwipes"] = roomSwipes.Count(),
                ["UniquePlayers"] = roomSwipes.Select(s => s.PlayerUsername).Distinct().Count(),
                ["UniqueStatements"] = roomSwipes.Select(s => s.StatementId).Distinct().Count(),
                ["RightSwipes"] = roomSwipes.Count(s => s.AgreeWithStatement),
                ["LeftSwipes"] = roomSwipes.Count(s => !s.AgreeWithStatement)
            };
            return stats;
        }
        public int GetStatisticValue(string roomCode, string statKey) {
            var stats = GetRoomStatistics(roomCode);
            if (stats.ContainsKey(statKey)) { return (int)stats[statKey]; }
            return 0;
        }
        private void LogRoomStatistics(string roomCode) {
            var totalSwipes = GetStatisticValue(roomCode, "TotalSwipes");
            var uniquePlayers = GetStatisticValue(roomCode, "UniquePlayers");
            var rightSwipes = GetStatisticValue(roomCode, "RightSwipes");
            var leftSwipes = GetStatisticValue(roomCode, "LeftSwipes");
            
            Console.WriteLine($"[Room {roomCode}] Total: {totalSwipes}, Players: {uniquePlayers}, Right: {rightSwipes}, Left: {leftSwipes}");
        }
    }