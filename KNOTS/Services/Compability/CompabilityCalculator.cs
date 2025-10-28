using System;
using System.Linq;
using System.Collections.Generic;
using KNOTS.Compability;

namespace KNOTS.Services.Compability;

/// <summary>
/// Calculates compatibility scores between players based on their swipes.
/// </summary>
/// <remarks>
/// Compatibility is determined by comparing how players respond to the same statements.
/// </remarks>
public class CompatibilityCalculator {
    private readonly SwipeRepository _swipeRepo;
    
    public CompatibilityCalculator(SwipeRepository swipeRepo) { 
        _swipeRepo = swipeRepo; 
    }
    
    /// <summary>
    /// Calculates compatibility between two players in a specific room.
    /// </summary>
    public CompatibilityScore Calculate(string roomCode, string player1, string player2) {
        var p1 = _swipeRepo.GetPlayerSwipes(roomCode, player1);
        var p2 = _swipeRepo.GetPlayerSwipes(roomCode, player2);

        // Rasti visus bendrus statement ID (į kuriuos abu žaidėjai atsakė)
        var commonStatementIds = p1.Select(s => s.StatementId)
            .Intersect(p2.Select(s => s.StatementId))
            .ToList();

        // Rasti sutampančius atsakymus tik tarp bendrų klausimų
        var matches = p1.Join(p2,
                s1 => s1.StatementId,
                s2 => s2.StatementId,
                (s1, s2) => new { s1, s2 })
            .Where(x => x.s1.AgreeWithStatement == x.s2.AgreeWithStatement)
            .Select(x => x.s1.StatementText)
            .ToList();

        // Naudoti bendrų klausimų skaičių, ne minimumą
        int totalCommonStatements = commonStatementIds.Count;

        return new CompatibilityScore(
            player1, 
            player2, 
            matches.Count, 
            totalCommonStatements,
            matches
        );
    }
    
    /// <summary>
    /// Calculates compatibility for all unique pairs of players in a room.
    /// </summary>
    public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames) {
        var results = new List<CompatibilityScore>();

        Console.WriteLine($"[CompatibilityCalculator] Calculating for {playerUsernames.Count} players in room {roomCode}");

        for (int i = 0; i < playerUsernames.Count; i++) {
            for (int j = i + 1; j < playerUsernames.Count; j++) {
                var score = Calculate(roomCode, playerUsernames[i], playerUsernames[j]);
                results.Add(score);
                Console.WriteLine($"  {score.Player1} & {score.Player2}: {score.Percentage}% ({score.MatchingSwipes}/{score.TotalStatements})");
            }
        }
        
        return results.OrderByDescending(r => r.Percentage).ToList();
    }
    
    /// <summary>
    /// Finds the best match (highest compatibility) among all players.
    /// </summary>
    public Dictionary<string, PlayerMatchInfo> GetBestMatchesForPlayers(List<CompatibilityScore> allResults) {
        var playerMatches = new Dictionary<string, PlayerMatchInfo>();
        
        // Gauti visus unikalius žaidėjus
        var allPlayers = allResults
            .SelectMany(r => new[] { r.Player1, r.Player2 })
            .Distinct()
            .ToList();
        
        foreach (var player in allPlayers) {
            // Rasti visus to žaidėjo match'us
            var playerResults = allResults
                .Where(r => r.Player1 == player || r.Player2 == player)
                .OrderByDescending(r => r.Percentage)
                .ToList();
            
            if (!playerResults.Any()) continue;
            
            var bestMatch = playerResults.First();
            var bestMatchPartner = bestMatch.Player1 == player ? bestMatch.Player2 : bestMatch.Player1;
            
            // Patikrinti ar šis žaidėjas buvo geriausias match'as savo partneriui
            var partnerBestMatch = allResults
                .Where(r => r.Player1 == bestMatchPartner || r.Player2 == bestMatchPartner)
                .OrderByDescending(r => r.Percentage)
                .FirstOrDefault();
            
            bool wasBestMatchForPartner = partnerBestMatch.TotalStatements > 0 && 
                (partnerBestMatch.Player1 == player || partnerBestMatch.Player2 == player);
            
            playerMatches[player] = new PlayerMatchInfo {
                BestMatchPartner = bestMatchPartner,
                BestMatchPercentage = bestMatch.Percentage,
                WasBestMatchForPartner = wasBestMatchForPartner,
                AllMatches = playerResults
            };
            
            Console.WriteLine($"[BestMatch] {player} -> {bestMatchPartner} ({bestMatch.Percentage}%) " +
                            $"[Mutual: {wasBestMatchForPartner}]");
        }
        
        return playerMatches;
    }
    
    /// <summary>
    /// Retrieves overall game statistics for a specific player based on all compatibility results.
    /// </summary>
    /// <param name="playerUsername">The username of the player whose statistics are being calculated.</param>
    /// <param name="allResults">A list of all <see cref="CompatibilityScore"/> results from the game session.</param>
    /// <returns>
    /// A <see cref="PlayerGameStatistics"/> instance containing the player’s performance data,
    /// including average compatibility, best match percentage, and match status.
    /// </returns>
    public PlayerGameStatistics GetPlayerStatistics(string playerUsername, List<CompatibilityScore> allResults) {
        var playerResults = allResults
            .Where(r => r.Player1 == playerUsername || r.Player2 == playerUsername)
            .ToList();
        
        if (!playerResults.Any()) {
            return new PlayerGameStatistics {
                PlayerUsername = playerUsername,
                GamesPlayed = 0,
                AverageCompatibility = 0,
                BestMatchPercentage = 0,
                WasBestMatch = false
            };
        }
        
        var bestMatchInfo = GetBestMatchesForPlayers(allResults);
        var playerInfo = bestMatchInfo.GetValueOrDefault(playerUsername);
        
        return new PlayerGameStatistics {
            PlayerUsername = playerUsername,
            GamesPlayed = 1, // Vienas game session
            AverageCompatibility = playerResults.Average(r => r.Percentage),
            BestMatchPercentage = playerInfo?.BestMatchPercentage ?? 0,
            WasBestMatch = playerInfo?.WasBestMatchForPartner ?? false
        };
    }
}

/// <summary>
/// Represents detailed information about a player’s best match in a game session.
/// </summary>
/// <remarks>
/// This class contains metadata about the player’s top compatibility partner,
/// including the percentage score and whether the relationship was mutual.
/// </remarks>
public class PlayerMatchInfo {
    /// <summary>
    /// Gets or sets the username of the player’s best match partner.
    /// </summary>
    public string BestMatchPartner { get; set; } = "";
    /// <summary>
    /// Gets or sets the compatibility percentage with the best match partner.
    /// </summary>
    public double BestMatchPercentage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this player was also the partner’s best match.
    /// </summary>
    public bool WasBestMatchForPartner { get; set; }
    /// <summary>
    /// Gets or sets all compatibility results associated with the player.
    /// </summary>
    public List<CompatibilityScore> AllMatches { get; set; } = new();
}

/// <summary>
/// Represents summarized game statistics for a single player during one game session.
/// </summary>
/// <remarks>
/// This model stores general performance metrics such as average compatibility,
/// best match data, and participation count.
/// </remarks>
public class PlayerGameStatistics {
    /// <summary>
    /// Gets or sets the username of the player.
    /// </summary>
    public string PlayerUsername { get; set; } = "";
    /// <summary>
    /// Gets or sets the number of games played by the player.
    /// </summary>
    public int GamesPlayed { get; set; }
    /// <summary>
    /// Gets or sets the player’s average compatibility percentage across all matches.
    /// </summary>
    public double AverageCompatibility { get; set; }
    /// <summary>
    /// Gets or sets the player’s best match percentage achieved in the session.
    /// </summary>
    public double BestMatchPercentage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the player was the best match for another participant.
    /// </summary>
    public bool WasBestMatch { get; set; }
}