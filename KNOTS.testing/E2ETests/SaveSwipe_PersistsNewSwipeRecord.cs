using System.Linq;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class SaveSwipePersistsNewSwipeRecord : EndToEndTestBase
{
    [Fact]
    public void SaveSwipe_PersistsNewSwipeRecord()
    {
        var roomCode = "ROOM-SAVE-1";
        var username = "player-one";
        var statement = new GameStatement { Id = "SW1", Text = "Swipe me", Topic = "General" };

        SeedStatements(new[] { statement });

        var saveResult = CompatibilityService.SaveSwipe(roomCode, username, statement.Id, swipeRight: true);

        Assert.True(saveResult);

        var stored = Context.PlayerSwipes.Single();
        Assert.Equal(roomCode, stored.RoomCode);
        Assert.Equal(username, stored.PlayerUsername);
        Assert.Equal(statement.Id, stored.StatementId);
        Assert.True(stored.AgreeWithStatement);
    }
}
