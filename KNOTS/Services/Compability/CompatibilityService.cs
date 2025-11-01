﻿using System.Text.Json;
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
        var allStatements = new List<GameStatement>
        {
            new GameStatement { Id = "D1", Text = "I like getting up early in the morning", Topic = "General" },
            new GameStatement { Id = "D2", Text = "I prefer relaxing at home over going to parties", Topic = "General" },
            new GameStatement { Id = "D3", Text = "I enjoy spontaneous trips", Topic = "General" },
            new GameStatement { Id = "D4", Text = "Animals are an important part of my life", Topic = "General" },
            new GameStatement { Id = "D5", Text = "I prefer movies over theater", Topic = "General" },
            new GameStatement { Id = "D6", Text = "Sports are part of my daily routine", Topic = "General" },
            new GameStatement { Id = "D7", Text = "I enjoy cooking at home", Topic = "General" },
            new GameStatement { Id = "D8", Text = "Summer is the best season", Topic = "General" },
            new GameStatement { Id = "D9", Text = "Meaningful conversations matter more to me than having fun", Topic = "General" },
            new GameStatement { Id = "D10", Text = "I like taking risks and trying new things", Topic = "General" },
            new GameStatement { Id = "D11", Text = "Music is an important part of my life", Topic = "General" },
            new GameStatement { Id = "D12", Text = "I value personal space in relationships", Topic = "General" },
            new GameStatement { Id = "D13", Text = "I like to plan everything in advance", Topic = "General" },
            new GameStatement { Id = "D14", Text = "I feel good at large parties", Topic = "General" },
            new GameStatement { Id = "D15", Text = "I live in the moment and don't worry about the future", Topic = "General" },
            new GameStatement { Id = "D16", Text = "Romantic relationships are important to me", Topic = "General" },
            new GameStatement { Id = "D18", Text = "Books are better than movies", Topic = "General" },
            new GameStatement { Id = "D19", Text = "I enjoy nature and hiking", Topic = "General" },
            new GameStatement { Id = "D20", Text = "Financial stability is a priority", Topic = "General" },

            // Topic: Science
            new GameStatement { Id = "F1", Text = "I like exploring new scientific concepts", Topic = "Science" },
            new GameStatement { Id = "F2", Text = "I enjoy reading about space and astronomy", Topic = "Science" },
            new GameStatement { Id = "F3", Text = "I like conducting experiments", Topic = "Science" },
            new GameStatement { Id = "F4", Text = "I follow the latest technology trends", Topic = "Science" },
            new GameStatement { Id = "F5", Text = "I am fascinated by physics", Topic = "Science" },
            new GameStatement { Id = "F6", Text = "I enjoy learning about biology", Topic = "Science" },
            new GameStatement { Id = "F7", Text = "I am interested in chemistry", Topic = "Science" },
            new GameStatement { Id = "F8", Text = "I like reading scientific journals", Topic = "Science" },
            new GameStatement { Id = "F9", Text = "I enjoy solving scientific puzzles", Topic = "Science" },
            new GameStatement { Id = "F10", Text = "I like watching science documentaries", Topic = "Science" },
            new GameStatement { Id = "F11", Text = "I enjoy attending science fairs", Topic = "Science" },
            new GameStatement { Id = "F12", Text = "I like solving complex math problems", Topic = "Science" },
            new GameStatement { Id = "F13", Text = "I enjoy learning about chemistry reactions", Topic = "Science" },
            new GameStatement { Id = "F14", Text = "I like reading about medical discoveries", Topic = "Science" },
            new GameStatement { Id = "S15", Text = "I enjoy studying the environment", Topic = "Science" },
            new GameStatement { Id = "F16", Text = "I like programming and computer science topics", Topic = "Science" },
            new GameStatement { Id = "F17", Text = "I enjoy discussing scientific theories", Topic = "Science" },
            new GameStatement { Id = "F18", Text = "I like attending lectures about space exploration", Topic = "Science" },
            new GameStatement { Id = "F19", Text = "I enjoy experimenting with physics concepts", Topic = "Science" },
            new GameStatement { Id = "F20", Text = "I like reading journals on technology advancements", Topic = "Science" },

            // Topic: Movies
            new GameStatement { Id = "M1", Text = "I prefer watching movies over reading books", Topic = "Movies" },
            new GameStatement { Id = "M2", Text = "I enjoy classic films", Topic = "Movies" },
            new GameStatement { Id = "M3", Text = "I love action movies", Topic = "Movies" },
            new GameStatement { Id = "M4", Text = "I enjoy watching comedies", Topic = "Movies" },
            new GameStatement { Id = "M5", Text = "I like indie films", Topic = "Movies" },
            new GameStatement { Id = "M6", Text = "I often watch documentaries", Topic = "Movies" },
            new GameStatement { Id = "M7", Text = "I enjoy animated movies", Topic = "Movies" },
            new GameStatement { Id = "M8", Text = "I like going to the cinema", Topic = "Movies" },
            new GameStatement { Id = "M9", Text = "I enjoy movie soundtracks", Topic = "Movies" },
            new GameStatement { Id = "M10", Text = "I like discussing movie plots with friends", Topic = "Movies" },
            new GameStatement { Id = "M11", Text = "I enjoy watching foreign films", Topic = "Movies" },
            new GameStatement { Id = "M12", Text = "I like binge-watching movie series", Topic = "Movies" },
            new GameStatement { Id = "M13", Text = "I enjoy film festivals", Topic = "Movies" },
            new GameStatement { Id = "M14", Text = "I like analyzing movie cinematography", Topic = "Movies" },
            new GameStatement { Id = "M15", Text = "I enjoy attending movie premieres", Topic = "Movies" },
            new GameStatement { Id = "M16", Text = "I like reading movie reviews", Topic = "Movies" },
            new GameStatement { Id = "M17", Text = "I enjoy discussing actors and directors", Topic = "Movies" },
            new GameStatement { Id = "M18", Text = "I like creating lists of my favorite movies", Topic = "Movies" },
            new GameStatement { Id = "M19", Text = "I enjoy watching film adaptations of books", Topic = "Movies" },
            new GameStatement { Id = "M20", Text = "I like exploring different movie genres", Topic = "Movies" },

            // Topic: Travel
            new GameStatement { Id = "T1", Text = "I love exploring new cities", Topic = "Travel" },
            new GameStatement { Id = "T2", Text = "I enjoy trying local cuisines while traveling", Topic = "Travel" },
            new GameStatement { Id = "T3", Text = "I like planning my trips in advance", Topic = "Travel" },
            new GameStatement { Id = "T4", Text = "I enjoy outdoor adventures", Topic = "Travel" },
            new GameStatement { Id = "T5", Text = "I like learning about other cultures", Topic = "Travel" },
            new GameStatement { Id = "T6", Text = "I enjoy road trips", Topic = "Travel" },
            new GameStatement { Id = "T7", Text = "I like visiting historical sites", Topic = "Travel" },
            new GameStatement { Id = "T8", Text = "I enjoy beach vacations", Topic = "Travel" },
            new GameStatement { Id = "T9", Text = "I like backpacking", Topic = "Travel" },
            new GameStatement { Id = "T10", Text = "I enjoy photographing landscapes", Topic = "Travel" },
            new GameStatement { Id = "T11", Text = "I enjoy traveling to off-the-beaten-path locations", Topic = "Travel" },
            new GameStatement { Id = "T12", Text = "I like learning local languages while traveling", Topic = "Travel" },
            new GameStatement { Id = "T13", Text = "I enjoy visiting museums abroad", Topic = "Travel" },
            new GameStatement { Id = "T14", Text = "I like planning weekend getaways", Topic = "Travel" },
            new GameStatement { Id = "T15", Text = "I enjoy trying adventure sports while traveling", Topic = "Travel" },
            new GameStatement { Id = "T16", Text = "I like taking scenic train journeys", Topic = "Travel" },
            new GameStatement { Id = "T17", Text = "I enjoy discovering hidden cafes and restaurants abroad", Topic = "Travel" },
            new GameStatement { Id = "T18", Text = "I like collecting souvenirs from trips", Topic = "Travel" },
            new GameStatement { Id = "T19", Text = "I enjoy traveling with friends or family", Topic = "Travel" },
            new GameStatement { Id = "T20", Text = "I like documenting my travels through photography or writing", Topic = "Travel" },

            //Topic: Hobbies
            new GameStatement { Id = "H1", Text = "I enjoy painting or drawing", Topic = "Hobbies" },
            new GameStatement { Id = "H2", Text = "I like playing musical instruments", Topic = "Hobbies" },
            new GameStatement { Id = "H3", Text = "I enjoy gardening", Topic = "Hobbies" },
            new GameStatement { Id = "H4", Text = "I like cooking or baking", Topic = "Hobbies" },
            new GameStatement { Id = "H5", Text = "I enjoy playing video games", Topic = "Hobbies" },
            new GameStatement { Id = "H6", Text = "I like knitting or crafting", Topic = "Hobbies" },
            new GameStatement { Id = "H7", Text = "I enjoy photography", Topic = "Hobbies" },
            new GameStatement { Id = "H8", Text = "I like reading novels", Topic = "Hobbies" },
            new GameStatement { Id = "H9", Text = "I enjoy exercising or sports", Topic = "Hobbies" },
            new GameStatement { Id = "H10", Text = "I like learning new skills in my free time", Topic = "Hobbies" },
            new GameStatement { Id = "H11", Text = "I enjoy woodworking or DIY projects", Topic = "Hobbies" },
            new GameStatement { Id = "H12", Text = "I like playing board games with friends", Topic = "Hobbies" },
            new GameStatement { Id = "H13", Text = "I enjoy knitting or sewing", Topic = "Hobbies" },
            new GameStatement { Id = "H14", Text = "I like practicing yoga or meditation", Topic = "Hobbies" },
            new GameStatement { Id = "H15", Text = "I enjoy learning a new language", Topic = "Hobbies" },
            new GameStatement { Id = "H16", Text = "I like baking new recipes", Topic = "Hobbies" },
            new GameStatement { Id = "H17", Text = "I enjoy writing short stories or poems", Topic = "Hobbies" },
            new GameStatement { Id = "H18", Text = "I like building models or puzzles", Topic = "Hobbies" },
            new GameStatement { Id = "H19", Text = "I enjoy playing card games", Topic = "Hobbies" },
            new GameStatement { Id = "H20", Text = "I like experimenting with photography techniques", Topic = "Hobbies" },
        };
        
        var existingIds = _context.Statements.Select(s => s.Id).ToHashSet();
        var newStatements = allStatements
            .Where(s => !existingIds.Contains(s.Id))
            .ToList();

        if (newStatements.Any())
        {
            _context.Statements.AddRange(newStatements);
            _context.SaveChanges();
            Console.WriteLine($"✅ Created {newStatements.Count} new statements in database.");
        }
        else
        {
            Console.WriteLine($"✅ All statements already exist ({existingIds.Count} total).");
        } ;
    }

    /// <summary>
    /// Retrieves or generates the list of statements for a specific room.
    /// Ensures all players in the same room receive identical questions.
    /// </summary>
    public List<GameStatement> GetRoomStatements(string roomCode, List<string>? selectedTopics = null, int count = 10) {
        Console.WriteLine("Selected topics: " + string.Join(", ", selectedTopics));
        var allDbTopics = _context.Statements.Select(s => s.Topic).Distinct().ToList();
        Console.WriteLine("DB Topics: " + string.Join(", ", allDbTopics));

        if (_roomStatements.ContainsKey(roomCode)) {
            Console.WriteLine($"✅ Returning existing {_roomStatements[roomCode].Count} statements for room {roomCode}");
            return _roomStatements[roomCode];
        }

        if (_roomStatements.ContainsKey(roomCode)) {
            Console.WriteLine($"✅ Returning existing {_roomStatements[roomCode].Count} statements for room {roomCode}");
            return _roomStatements[roomCode];
        }

        var random = new Random();
        List<GameStatement> statements;

        if (selectedTopics != null && selectedTopics.Any()) {

            var filteredStatements = _context.Statements
                .Where(s => selectedTopics.Contains(s.Topic))
                .ToList();
            
            statements = filteredStatements
                .OrderBy(x => random.Next())
                .Take(Math.Min(count, filteredStatements.Count))
                .ToList();
        }
        else {
            var allStatements = _context.Statements.ToList();
            statements = allStatements
                .OrderBy(x => random.Next())
                .Take(Math.Min(count, allStatements.Count))
                .ToList();
        }

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