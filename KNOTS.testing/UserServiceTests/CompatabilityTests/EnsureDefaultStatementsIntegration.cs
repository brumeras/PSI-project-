using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class EnsureDefaultStatementsIntegration : UserServiceTestBase 
{
    [Fact]
    public void EnsureDefaultStatements_EnsureStatementsAdded() 
    {
        Assert.Empty(Context.Statements);
        
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var compatService = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        Assert.Contains(Context.Statements, s => s.Id == "D1");
        Assert.Contains(Context.Statements, s => s.Id == "F2");
        Assert.Contains(Context.Statements, s => s.Id == "M3");
        Assert.Contains(Context.Statements, s => s.Id == "T4");
        Assert.Contains(Context.Statements, s => s.Id == "H5");
    }
}