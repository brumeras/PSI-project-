using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

//BUTINIA PAKEISTI STATEMENT ID TESTUOJANT I TOKI KURIO NERA DB (pirma kart testuojant) - ENSUREDEFAULTSTATEMENTS BY DEFAULT IMETA STATEMENTS, TAI REIK TOKIO KURIO NEIMETA
public class SaveSwipeIntegration : UserServiceTestBase
{
    [Fact]
    public void SaveSwipe_AddsSwipe() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        var statement = new GameStatement { Id = "X1", Text = "statement text", Topic = "General" };
        Context.Statements.Add(statement);
        Context.SaveChanges();
        
        var res = service.SaveSwipe("room1", "player1", "X1", true);
        
        Assert.True(res);
        var saved = Context.PlayerSwipes.FirstOrDefault();
        Assert.NotNull(saved);
        Assert.Equal("room1", saved.RoomCode);
        Assert.Equal("player1", saved.PlayerUsername);
        Assert.Equal("X1", saved.StatementId);
        Assert.True(saved.AgreeWithStatement);
    }
}