using KNOTS.Compability;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Services.Compability;
using System.Text.Json;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetPlayerHistoryIntegration : UserServiceTestBase
{
    [Fact]
    public void GetPlayerHistory_OnlyEntries() 
    {
        var player1 = "pirmas";
        var player2 = "antras";
        var history1 = new GameHistoryRecord
        {
            RoomCode = "room1",
            PlayedDate = DateTime.Now,
            TotalPlayers = 2,
            BestMatchPlayer = player2,
            BestMatchPercentage = 90,
            PlayerUsernames = JsonSerializer.Serialize(new List<string> { player1, player2 }),
            ResultsJson = JsonSerializer.Serialize(new List<CompatibilityScore> { new CompatibilityScore(player1, player2, 1, 1, new List<string> { "statement1" }) })
        };
        Context.GameHistory.Add(history1);
        Context.SaveChanges();
        
        InterfaceLoggingService loggingService = new LoggingService();
        InterfaceUserService userService = new UserService(Context, loggingService);
        InterfaceSwipeRepository swipeRepository = new SwipeRepository(Context);
        InterfaceCompatibilityCalculator calculator = new CompatibilityCalculator(swipeRepository);
        
        var service = new CompatibilityService(Context, userService, swipeRepository, calculator, loggingService);
        var res = service.GetPlayerHistory(player1);
        
        Assert.Single(res);
        var entry = res[0];
        Assert.Equal("room1", entry.RoomCode);
        Assert.Equal(player2, entry.BestMatchPlayer);
        Assert.Equal(2, entry.TotalPlayers);
        
        Assert.Single(entry.AllResults);
        var score = entry.AllResults[0];
        Assert.Equal(player1, score.Player1);
        Assert.Equal(player2, score.Player2);
        Assert.Equal(1, score.MatchingSwipes);
        Assert.Equal(1, score.TotalStatements);
        Assert.Single(score.MatchedStatements);
        Assert.Equal(100, score.Percentage);
    }
}