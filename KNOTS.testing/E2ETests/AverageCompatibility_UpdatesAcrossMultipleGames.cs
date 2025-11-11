using System.Collections.Generic;
using System.Linq;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class AverageCompatibilityUpdatesAcrossMultipleGames : EndToEndTestBase
{
    [Fact]
    public void SaveGameToHistory_CalculatesAverageCompatibilityOverMultipleSessions()
    {
        RegisterTestUsers("alpha", "beta");

        var roomCode = "ROOM-AVG";

        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "AVG-1", Text = "Average game one", Topic = "General" }
        });

        CompatibilityService.SaveSwipe(roomCode, "alpha", "AVG-1", true);
        CompatibilityService.SaveSwipe(roomCode, "beta", "AVG-1", true);
        CompatibilityService.SaveGameToHistory(roomCode, new List<string> { "alpha", "beta" });

        CompatibilityService.ClearRoomData(roomCode);

        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "AVG-2", Text = "Average game two", Topic = "General" }
        });

        CompatibilityService.SaveSwipe(roomCode, "alpha", "AVG-2", true);
        CompatibilityService.SaveSwipe(roomCode, "beta", "AVG-2", false);
        CompatibilityService.SaveGameToHistory(roomCode, new List<string> { "alpha", "beta" });

        var alpha = Context.Users.Single(u => u.Username == "alpha");
        var beta = Context.Users.Single(u => u.Username == "beta");

        Assert.Equal(2, alpha.TotalGamesPlayed);
        Assert.Equal(2, beta.TotalGamesPlayed);
        Assert.Equal(50.0, alpha.AverageCompatibilityScore);
        Assert.Equal(50.0, beta.AverageCompatibilityScore);
        Assert.Equal(2, alpha.BestMatchesCount);
        Assert.Equal(2, beta.BestMatchesCount);
        Assert.Equal(2, Context.GameHistory.Count());
    }
}
