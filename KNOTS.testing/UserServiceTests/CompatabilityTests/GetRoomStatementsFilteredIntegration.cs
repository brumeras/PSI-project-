using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetRoomStatementsFilteredIntegration : UserServiceTestBase
{
    [Fact]
    public void GetRoomStatemenets_Filtered() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        var res = service.GetRoomStatements("roomTopic", new List<string>{"Science"}, 10);
        Assert.NotEmpty(res);
        Assert.All(res, x => Assert.Equal("Science", x.Topic));
    }
}