using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Compability;
using Microsoft.Identity.Client;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class SaveGameToHistoryIntegration : UserServiceTestBase{
    [Fact]
    public void SaveGameToHistory_SavesAndUpdates() {
        Context.Users.AddRange(
            new User{Username = "pirmas", PasswordHash = "hash1"},
            new User{Username = "antras", PasswordHash = "hash2"}
            );
        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord {RoomCode = "room1", PlayerUsername = "pirmas", StatementId = "s1", StatementText = "statement1", AgreeWithStatement = true },
            new PlayerSwipeRecord {RoomCode = "room1", PlayerUsername = "antras", StatementId = "s1", StatementText = "statement1", AgreeWithStatement = true }
        );
        Context.SaveChanges();
        
        var userService = new UserService(Context, new LoggingService());
        var service = new CompatibilityService(Context, userService);
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