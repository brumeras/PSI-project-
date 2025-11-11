using System.Collections.Generic;
using System.Linq;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class GetPlayerSwipesReturnsPlayerHistory : EndToEndTestBase
{
    [Fact]
    public void GetPlayerSwipes_ReturnsAllSwipesForPlayer()
    {
        var roomCode = "ROOM-SWIPES";
        var username = "player-three";

        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "SW3-1", Text = "Statement 1", Topic = "General" },
            new GameStatement { Id = "SW3-2", Text = "Statement 2", Topic = "General" },
        });

        CompatibilityService.SaveSwipe(roomCode, username, "SW3-1", swipeRight: true);
        CompatibilityService.SaveSwipe(roomCode, username, "SW3-2", swipeRight: false);

        var swipes = CompatibilityService.GetPlayerSwipes(roomCode, username);

        Assert.Equal(2, swipes.Count);
        Assert.Equal(
            new[] { "SW3-1", "SW3-2" },
            swipes.Select(s => s.StatementId).OrderBy(id => id)
        );
        Assert.Contains(swipes, s => s.AgreeWithStatement);
        Assert.Contains(swipes, s => !s.AgreeWithStatement);
    }
}