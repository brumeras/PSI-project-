using KNOTS.Compability;

namespace KNOTS.Services.Compability;

public class CompatibilityCalculator
{
    private readonly SwipeRepository _swipeRepo;

    public CompatibilityCalculator(SwipeRepository swipeRepo)
    {
        _swipeRepo = swipeRepo;
    }

    public CompatibilityScore Calculate(string roomCode, string player1, string player2)
    {
        var p1 = _swipeRepo.GetPlayerSwipes(roomCode, player1);
        var p2 = _swipeRepo.GetPlayerSwipes(roomCode, player2);

        var matches = p1.Join(p2,
                s1 => s1.StatementId,
                s2 => s2.StatementId,
                (s1, s2) => new { s1, s2 })
            .Where(x => x.s1.AgreeWithStatement == x.s2.AgreeWithStatement)
            .Select(x => x.s1.StatementText)
            .ToList();

        return new CompatibilityScore(player1, player2, matches.Count, Math.Min(p1.Count, p2.Count), matches);
    }
}
