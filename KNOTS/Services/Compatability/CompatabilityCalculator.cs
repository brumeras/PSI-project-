using System;
using System.Linq;
using System.Collections.Generic;
using KNOTS.Compability;

namespace KNOTS.Services.Compability;

public class CompatibilityCalculator {
    private readonly SwipeRepository _swipeRepo;
    public CompatibilityCalculator(SwipeRepository swipeRepo) {_swipeRepo = swipeRepo;}
    public CompatibilityScore Calculate(string roomCode, string player1, string player2) {
        var p1 = _swipeRepo.GetPlayerSwipes(roomCode, player1);
        var p2 = _swipeRepo.GetPlayerSwipes(roomCode, player2);
        
        var commonStatementIds = p1.Select(s => s.StatementId)
            .Intersect(p2.Select(s => s.StatementId))
            .ToList();
        var matches = p1.Join(p2,
                s1 => s1.StatementId,
                s2 => s2.StatementId,
                (s1, s2) => new { s1, s2 })
            .Where(x => x.s1.AgreeWithStatement == x.s2.AgreeWithStatement)
            .Select(x => x.s1.StatementText)
            .ToList();
        int totalCommonStatements = commonStatementIds.Count;
        return new CompatibilityScore(player1, player2, matches.Count, totalCommonStatements, matches);
    }
    public List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames) {
        var results = new List<CompatibilityScore>();
        for (int i = 0; i < playerUsernames.Count; i++) {
            for (int j = i + 1; j < playerUsernames.Count; j++) {
                results.Add(Calculate(roomCode, playerUsernames[i], playerUsernames[j]));
            }
        }
        return results.OrderByDescending(r => r.Percentage).ToList();
    }
    public Dictionary<string, PlayerMatchInfo> GetBestMatchesForPlayers(List<CompatibilityScore> allResults) {
        var playerMatches = new Dictionary<string, PlayerMatchInfo>();
        var allPlayers = allResults.SelectMany(r => new[] { r.Player1, r.Player2 }).Distinct().ToList();
        
        foreach (var player in allPlayers) {
            var playerResults = allResults
                .Where(r => r.Player1 == player || r.Player2 == player)
                .OrderByDescending(r => r.Percentage)
                .ToList();
            
            if (!playerResults.Any()) continue;
            
            var bestMatch = playerResults.First();
            var bestMatchPartner = bestMatch.Player1 == player ? bestMatch.Player2 : bestMatch.Player1;
 
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
        }
        return playerMatches;
    }
    public PlayerGameStatistics GetPlayerStatistics(string playerUsername, List<CompatibilityScore> allResults) {
        var playerResults = allResults.Where(r => r.Player1 == playerUsername || r.Player2 == playerUsername).ToList();
        if (!playerResults.Any()) {return new PlayerGameStatistics {PlayerUsername = playerUsername};}
        
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
