using System.Collections.Generic;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class HaveAllPlayersFinishedReturnsTrueWhenAllPlayersMeetRequiredSwipes : EndToEndTestBase
{
    [Fact]
    public void HaveAllPlayersFinished_ReturnsTrue_WhenAllPlayersMeetRequiredSwipes()
    {
        var roomCode = "ROOM-PROGRESS-1";
        var players = new List<string> { "player-one", "player-two" };

        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "HP1", Text = "Statement 1", Topic = "General" },
            new GameStatement { Id = "HP2", Text = "Statement 2", Topic = "General" },
        });

        foreach (var player in players)
        {
            CompatibilityService.SaveSwipe(roomCode, player, "HP1", swipeRight: true);
            CompatibilityService.SaveSwipe(roomCode, player, "HP2", swipeRight: true);
        }

        var finished = CompatibilityService.HaveAllPlayersFinished(roomCode, players, totalStatements: 2);

        Assert.True(finished);
    }
}
