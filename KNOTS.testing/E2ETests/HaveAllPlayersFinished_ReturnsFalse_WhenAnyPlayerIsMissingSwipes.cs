using System.Collections.Generic;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class HaveAllPlayersFinishedReturnsFalseWhenAnyPlayerIsMissingSwipes : EndToEndTestBase
{
    [Fact]
    public void HaveAllPlayersFinished_ReturnsFalse_WhenAnyPlayerIsMissingSwipes()
    {
        var roomCode = "ROOM-PROGRESS-2";
        var players = new List<string> { "player-one", "player-two" };

        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "HP3", Text = "Statement 3", Topic = "General" },
            new GameStatement { Id = "HP4", Text = "Statement 4", Topic = "General" },
        });

        CompatibilityService.SaveSwipe(roomCode, players[0], "HP3", swipeRight: true);
        CompatibilityService.SaveSwipe(roomCode, players[0], "HP4", swipeRight: false);
        CompatibilityService.SaveSwipe(roomCode, players[1], "HP3", swipeRight: true);

        var finished = CompatibilityService.HaveAllPlayersFinished(roomCode, players, totalStatements: 2);

        Assert.False(finished);
    }
}
