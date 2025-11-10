using System.Diagnostics.Contracts;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class CalculateAllCompatibilitiesIntegration : UserServiceTestBase
{
    [Fact]
    public void CalculateAllCompatibilities_ReturnsCorrectPairwiseResultsINTEGRATION()
    {
        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord { PlayerUsername = "pirmas", StatementId = "s1", StatementText = "statement 1", AgreeWithStatement = true, RoomCode = "room1" },
            new PlayerSwipeRecord { PlayerUsername = "pirmas", StatementId = "s2", StatementText = "statement 2", AgreeWithStatement = false, RoomCode = "room1" },
            new PlayerSwipeRecord { PlayerUsername = "antras", StatementId = "s1", StatementText = "statement 1", AgreeWithStatement = true, RoomCode = "room1" },
            new PlayerSwipeRecord { PlayerUsername = "antras", StatementId = "s2", StatementText = "statement 2", AgreeWithStatement = true, RoomCode = "room1" }
        );
        Context.SaveChanges();
        var repo = new SwipeRepository(Context);
        var calculator = new CompatibilityCalculator(repo);
        var players = new List<string> { "pirmas", "antras" };
        var res = calculator.CalculateAllCompatibilities("room1", players);

        Assert.Single(res);
        var result = res.First();
        Assert.Equal("pirmas", result.Player1);
        Assert.Equal("antras", result.Player2);
        Assert.Equal(2, result.TotalStatements);
        Assert.Equal(1, result.MatchingSwipes);
        Assert.Equal(50, result.Percentage);
    }
}

            