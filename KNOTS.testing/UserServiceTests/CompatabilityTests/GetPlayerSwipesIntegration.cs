using KNOTS.Models;
using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetPlayerSwipesIntegration : UserServiceTestBase{
    [Fact]
    public void GetPlayerSwipes_ReturnsCorrectSwipes() {
        var service = new CompatibilityService(Context, UserService);
        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord{RoomCode = "room1", PlayerUsername = "player1", StatementId = "s1", StatementText = "A", AgreeWithStatement = true, SwipedAt = DateTime.Now},
            new PlayerSwipeRecord{RoomCode = "room1", PlayerUsername = "player1", StatementId = "s2", StatementText = "B", AgreeWithStatement = true, SwipedAt = DateTime.Now}
            );
        Context.SaveChanges();
        var res = service.GetPlayerSwipes("room1", "player1");
        Assert.Equal(2, res.Count);
        Assert.Contains(res, r => r.StatementId == "s1");
        Assert.Contains(res, r => r.StatementId == "s2");
    }
}