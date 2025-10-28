using System.Text.Json;
using KNOTS.Compability;
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Compability;

namespace KNOTS.Services;

/// <summary>
/// Provides operations for managing player swipes, calculating compatibility scores,
/// tracking game progress, and storing completed game sessions.
/// </summary>
/// <remarks>
/// Acts as the main coordination layer between repositories and domain services,
/// such as <see cref="CompatibilityCalculator"/>, <see cref="StatisticsService"/>, 
/// and <see cref="GameHistoryService"/>.
/// </remarks>
public class CompatibilityService {
    private readonly AppDbContext _context;
    private readonly UserService _userService;
    private readonly CompatibilityCalculator _calculator;
    
    // Saugoti kambario klausimus atmintinėje
    private static Dictionary<string, List<GameStatement>> _roomStatements = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CompatibilityService"/> class
    /// and ensures default statements exist in the database.
    /// </summary>
    public CompatibilityService(AppDbContext context, UserService userService) {
        _context = context;
        _userService = userService;
        _calculator = new CompatibilityCalculator(new SwipeRepository(context));
        
        Console.WriteLine("🔧 CompatibilityService created with DB context");
        EnsureDefaultStatements();
    }
    
    /// <summary>
    /// Ensures that the database contains the default set of statements.
    /// If no statements exist, they are created and saved.
    /// </summary>
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
                new GameStatement { Id = "S9", Text = "Meaningful conversations matter more to me than having fun" },
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
        else { 
            Console.WriteLine($"✅ Statements already exist in database ({_context.Statements.Count()} total)"); 
        }
    }
    
    /// <summary>
    /// Retrieves or generates the list of statements for a specific room.
    /// Ensures all players in the same room receive identical questions.
    /// </summary>
    public List<GameStatement> GetRoomStatements(string roomCode, int count = 10)
    {
        if (_roomStatements.ContainsKey(roomCode))
        {
            Console.WriteLine($"✅ Returning existing {_roomStatements[roomCode].Count} statements for room {roomCode}");
            return _roomStatements[roomCode];
        }
        
        var random = new Random();
        var statements = _context.Statements
            .AsEnumerable()
            .OrderBy(x => random.Next())
            .Take(Math.Min(count, _context.Statements.Count()))
            .ToList();
        
        _roomStatements[roomCode] = statements;
        Console.WriteLine($"✅ Created new {statements.Count} statements for room {roomCode}");
        Console.WriteLine($"📋 Statement IDs: {string.Join(", ", statements.Select(s => s.Id))}");
        
        return statements;
    }
    
    /// <summary>
    /// Returns a random list of statements.
    /// </summary>
    /// <remarks>
    /// This method is obsolete — use <see cref="GetRoomStatements"/> instead
    /// to ensure all players in the same room receive identical questions.
    /// </remarks>
    [Obsolete("Use GetRoomStatements(roomCode) instead to ensure all players get same questions")]
    public List<GameStatement> GetRandomStatements(int count)
    {
        var random = new Random();
        return _context.Statements
            .AsEnumerable()
            .OrderBy(x => random.Next())
            .Take(Math.Min(count, _context.Statements.Count()))
            .ToList();
    }
    
    
    /// <summary>
    /// Saves a player's swipe (response) to the database.
    /// </summary>
    /// <returns>True if the swipe was successfully saved; otherwise, false.</returns>
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
            
            if (existing != null) { 
                _context.PlayerSwipes.Remove(existing); 
            }

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
    
    /// <summary>
    /// Retrieves all swipes made in the specified room.
    /// </summary>
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
    
    /// <summary>
    /// Retrieves all swipes made by a specific player in a given room.
    /// </summary>
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
    
    /// <summary>
    /// Checks if all players in the room have completed their swipes.
    /// </summary>
    public bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements) {
        var uniquePlayers = GetStatisticValue(roomCode, "UniquePlayers");
        var totalSwipes = GetStatisticValue(roomCode, "TotalSwipes");
        
        Console.WriteLine($"[HaveAllPlayersFinished] Room {roomCode}: {uniquePlayers} players, {totalSwipes} total swipes");

        foreach (var player in playerUsernames) {
            var swipeCount = _context.PlayerSwipes
                .Count(s => s.RoomCode == roomCode && s.PlayerUsername == player);
            if (swipeCount < totalStatements) {
                return false; 
            }
        }

        return true;
    }
    
    /// <summary>
    /// Calculates compatibility scores for all players in the specified room.
    /// </summary>
    public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames) {
        Console.WriteLine($"[CalculateAllCompatibilities] Starting calculation for room {roomCode}");
        LogRoomStatistics(roomCode);
        
        return _calculator.CalculateAllCompatibilities(roomCode, playerUsernames);
    }
    
    /// <summary>
    /// Saves the current game session to history and updates player statistics.
    /// </summary>
    public void SaveGameToHistory(string roomCode, List<string> playerUsernames) {
        try {
            Console.WriteLine($"[SaveGameToHistory] Saving game for room {roomCode}");
            LogRoomStatistics(roomCode);
            
            var allResults = CalculateAllCompatibilities(roomCode, playerUsernames);
            if (!allResults.Any()) {
                Console.WriteLine("⚠️ No results to save");
                return;
            }

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
            
            // Naudoti CompatibilityCalculator logiką vietoj sudėtingo ciklo
            foreach (var player in playerUsernames) {
                var stats = _calculator.GetPlayerStatistics(player, allResults);
                _userService.UpdateUserStatistics(
                    stats.PlayerUsername, 
                    stats.BestMatchPercentage, 
                    stats.WasBestMatch
                );
                
                Console.WriteLine($"📊 Updated stats for {player}: Best={stats.BestMatchPercentage:F2}%, WasBest={stats.WasBestMatch}");
            }
            
            Console.WriteLine($"✅ Game history saved for room {roomCode}");
        }
        catch (Exception ex) { 
            Console.WriteLine($"❌ Error saving game history: {ex.Message}\n{ex.StackTrace}"); 
        }
    }
    
    /// <summary>
    /// Retrieves all past games in which the specified player participated.
    /// </summary>
    public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) {
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
    
    /// <summary>
    /// Retrieves the entire game history for all rooms.
    /// </summary>
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
            })
            .ToList();
    }
    
    /// <summary>
    /// Clears all data related to the specified room from both the database and cache.
    /// </summary>
    /// <remarks>
    /// Does not save the session to history — use <see cref="SaveGameToHistory"/> for that.
    /// </remarks>
    public void ClearRoomData(string roomCode) {
        var swipesToRemove = _context.PlayerSwipes.Where(s => s.RoomCode == roomCode);
        _context.PlayerSwipes.RemoveRange(swipesToRemove);
        _context.SaveChanges();
        
        // Išvalyti kambario klausimus iš cache
        if (_roomStatements.ContainsKey(roomCode))
        {
            _roomStatements.Remove(roomCode);
            Console.WriteLine($"✅ Cleared room statements cache for {roomCode}");
        }
        
        Console.WriteLine($"✅ Cleared swipe data for room {roomCode}");
    }

    /// <summary>
    /// Retrieves statistical data about a specific room, including swipe counts and unique players.
    /// </summary>
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
    
    /// <summary>
    /// Retrieves a specific numerical statistic value for a given room.
    /// </summary>
    public int GetStatisticValue(string roomCode, string statKey) {
        var stats = GetRoomStatistics(roomCode);
        if (stats.ContainsKey(statKey)) { 
            return (int)stats[statKey]; 
        }
        return 0;
    }
    
    /// <summary>
    /// Logs room statistics to the console for debugging and monitoring.
    /// </summary>
    private void LogRoomStatistics(string roomCode) {
        var totalSwipes = GetStatisticValue(roomCode, "TotalSwipes");
        var uniquePlayers = GetStatisticValue(roomCode, "UniquePlayers");
        var rightSwipes = GetStatisticValue(roomCode, "RightSwipes");
        var leftSwipes = GetStatisticValue(roomCode, "LeftSwipes");
        
        Console.WriteLine($"[Room {roomCode}] Total: {totalSwipes}, Players: {uniquePlayers}, Right: {rightSwipes}, Left: {leftSwipes}");
    }
}