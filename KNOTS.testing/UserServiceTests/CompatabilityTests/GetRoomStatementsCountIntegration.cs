using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetRoomStatementsCountIntegration : UserServiceTestBase
{
    [Fact]
    public void GetRoomStatements_Count() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        var many = service.GetRoomStatements("roomCount", null, 5);
        Assert.True(many.Count <= 5);
        
        //cia atvejis, kai nera kazkokio topic, tai grazinam tiesiog kas egzistuoja
        var sml = service.GetRoomStatements("room", new List<string>{"someTopic"}, 10);
        Assert.True(sml.Count <= 10);
    }
}