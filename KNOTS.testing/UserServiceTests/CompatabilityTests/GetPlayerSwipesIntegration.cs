using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetPlayerSwipesIntegration : UserServiceTestBase
{
    [Fact]
    public void GetPlayerSwipes_ReturnsCorrectSwipes() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
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