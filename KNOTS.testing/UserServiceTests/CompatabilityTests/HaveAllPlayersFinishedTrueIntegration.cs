using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class HaveAllPlayersFinishedTrueIntegration : UserServiceTestBase 
{
    [Fact]
    public void HaveAllPlayersFinishedTrue() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        var room = "room";
        var players = new List<string> {"player1", "player2"};
        var totalSt = 2;

        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player1", StatementId = "s1" },
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player1", StatementId = "s2" },
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player2", StatementId = "s1" },
            new PlayerSwipeRecord { RoomCode = room, PlayerUsername = "player2", StatementId = "s2" }
        );
        Context.SaveChanges();
        
        var res = service.HaveAllPlayersFinished(room, players, totalSt);
        Assert.True(res);
    }
}