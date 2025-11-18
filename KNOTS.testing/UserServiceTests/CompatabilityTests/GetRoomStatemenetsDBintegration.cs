using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetRoomStatemenetsDBintegration : UserServiceTestBase 
{
    [Fact]
    public void GetRoomStatemenets_EnsuresReadFromDB() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        var res = service.GetRoomStatements("room1", null, 10);
        Assert.NotEmpty(res);
        Assert.True(Context.Statements.Any());
        Assert.All(res, s => Assert.Contains(s.Id, Context.Statements.Select(x => x.Id)));
    }
}