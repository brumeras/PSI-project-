using System.Linq;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class SaveSwipeOverwritesExistingSwipeForSameStatement : EndToEndTestBase
{
    [Fact]
    public void SaveSwipe_OverwritesExistingSwipeForSameStatement()
    {
        var roomCode = "ROOM-SAVE-2";
        var username = "player-two";
        var statement = new GameStatement { Id = "SW2", Text = "Swipe me again", Topic = "General" };

        SeedStatements(new[] { statement });

        var firstResult = CompatibilityService.SaveSwipe(roomCode, username, statement.Id, swipeRight: true);
        var secondResult = CompatibilityService.SaveSwipe(roomCode, username, statement.Id, swipeRight: false);

        Assert.True(firstResult);
        Assert.True(secondResult);

        var stored = Context.PlayerSwipes.Single();
        Assert.Equal(roomCode, stored.RoomCode);
        Assert.Equal(username, stored.PlayerUsername);
        Assert.Equal(statement.Id, stored.StatementId);
        Assert.False(stored.AgreeWithStatement);
    }
}
