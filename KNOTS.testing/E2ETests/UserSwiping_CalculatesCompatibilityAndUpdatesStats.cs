using System.Collections.Generic;
using System.Linq;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class UserSwipingCalculatesCompatibilityAndUpdatesStats : EndToEndTestBase
{
    [Fact]
    public void UserSwiping_CompatibilityCalculation_UpdatesUserStatistics()
    {
        RegisterTestUsers("alice", "bob");

        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "FLOW-1", Text = "Flow statement", Topic = "General" }
        });

        var roomCode = "ROOM-FLOW";
        CompatibilityService.SaveSwipe(roomCode, "alice", "FLOW-1", true);
        CompatibilityService.SaveSwipe(roomCode, "bob", "FLOW-1", true);

        CompatibilityService.SaveGameToHistory(roomCode, new List<string> { "alice", "bob" });

        var history = Context.GameHistory.Single();
        Assert.Equal(roomCode, history.RoomCode);
        Assert.Equal(2, history.TotalPlayers);
        Assert.Equal(100.0, history.BestMatchPercentage);

        var alice = Context.Users.Single(u => u.Username == "alice");
        var bob = Context.Users.Single(u => u.Username == "bob");

        Assert.Equal(1, alice.TotalGamesPlayed);
        Assert.Equal(1, bob.TotalGamesPlayed);
        Assert.Equal(100.0, alice.AverageCompatibilityScore);
        Assert.Equal(100.0, bob.AverageCompatibilityScore);
        Assert.Equal(1, alice.BestMatchesCount);
        Assert.Equal(1, bob.BestMatchesCount);
    }
}
