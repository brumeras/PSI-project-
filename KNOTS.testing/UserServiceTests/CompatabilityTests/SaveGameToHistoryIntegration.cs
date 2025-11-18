using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class SaveGameToHistoryIntegration : UserServiceTestBase
{
    [Fact]
    public void SaveGameToHistory_SavesAndUpdates() 
    {
        InterfaceLoggingService loggingService = new LoggingService();
        
        Context.Users.AddRange(
            new User{Username = "pirmas", PasswordHash = "hash1"},
            new User{Username = "antras", PasswordHash = "hash2"}
        );
        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord {RoomCode = "room1", PlayerUsername = "pirmas", StatementId = "s1", StatementText = "statement1", AgreeWithStatement = true },
            new PlayerSwipeRecord {RoomCode = "room1", PlayerUsername = "antras", StatementId = "s1", StatementText = "statement1", AgreeWithStatement = true }
        );
        Context.SaveChanges();
        
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        
        var roomCode = "room1";
        var players = new List<string>{"pirmas", "antras"};
        
        service.SaveGameToHistory(roomCode, players);
        var saved = Context.GameHistory.FirstOrDefault(h => h.RoomCode == roomCode);
        Assert.NotNull(saved);
        Assert.Equal(players.Count, saved.TotalPlayers);
        Assert.Contains("pirmas", saved.PlayerUsernames);
        Assert.NotEmpty(saved.ResultsJson);
        Assert.NotNull(saved.BestMatchPlayer);
    }
}