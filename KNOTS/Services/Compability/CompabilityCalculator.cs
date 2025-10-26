using System;
using System.Linq;
using KNOTS.Compability;

namespace KNOTS.Services.Compability;

public class CompatibilityCalculator {
    private readonly SwipeRepository _swipeRepo;
    public CompatibilityCalculator(SwipeRepository swipeRepo) { _swipeRepo = swipeRepo; }
    
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
            totalCommonStatements,  // Pakeista iš Math.Min į tikrą bendrų klausimų skaičių
            matches
        );
    }
}