using KNOTS.Models;
using KNOTS.Services.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class CalculateIntegration : UserServiceTestBase {
    [Fact]
    public void Calculate_TwoPlayersWithMatchingSwipes_ReturnsCorectINTEGRATION() {
        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord { PlayerUsername = "pirmas", StatementId = "s1", StatementText = "pirmas statement", AgreeWithStatement = true, RoomCode = "room1" }, 
            new PlayerSwipeRecord { PlayerUsername = "pirmas", StatementId = "s2", StatementText = "antras statement", AgreeWithStatement = true, RoomCode = "room1" },
            new PlayerSwipeRecord { PlayerUsername = "antras", StatementId = "s1", StatementText = "pirmas statement", AgreeWithStatement = false, RoomCode = "room1" }, 
            new PlayerSwipeRecord { PlayerUsername = "antras", StatementId = "s2", StatementText = "antras statement", AgreeWithStatement = true, RoomCode = "room1" }
        );
        Context.SaveChanges();
        var repo = new SwipeRepository(Context);
        var calculator = new CompatibilityCalculator(repo);
        var res = calculator.Calculate("room1", "pirmas", "antras");
        Assert.Equal(2, res.TotalStatements); 
        Assert.Equal(1, res.MatchingSwipes);
        Assert.Equal(50, res.Percentage);
    }
}