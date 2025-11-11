using KNOTS.Models;
using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class HaveAllPlayersFinishedFalseIntegration : UserServiceTestBase {
    [Fact]
    public void HaveAllPlayersFinishedFalse() {
        var service = new CompatibilityService(Context, UserService);
        var room = "room";
        var players = new List<string> {"player1", "player2"};
        var totalSt = 2;

        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player1", StatementId = "s1" },
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player1", StatementId = "s2" },
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player2", StatementId = "s1" }
        );
        Context.SaveChanges();
        var res = service.HaveAllPlayersFinished(room, players, totalSt);
        Assert.False(res);
    }
}