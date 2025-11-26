using Bunit;
using KNOTS.Components.Pages;
using System.Collections.Generic;

public class Game_ShowResultsTests : GameTestBase
{
    [Fact]
    public void ShowResults_SetsFlagAndClearsStatusWhenAllFinished()
    {
        // Arrange
        var (component, _, _, mockCompatibility, mockGameRoom) = SetupGameComponent(currentUser: "Henry");

        // Setup that all players finished
        var players = new List<string> { "Henry" };
        component.Instance.currentRoomCode = "ROOM1";
        component.Instance.playersInRoom. AddRange(players);
        component.Instance.allPlayersFinished = true;

        mockGameRoom.Setup(r => r.GetRoomPlayerUsernames("ROOM1"))
            .Returns(players);
        
        mockCompatibility.Setup(c => c.HaveAllPlayersFinished("ROOM1", players, 10))
            .Returns(true);

        // Act
        component. Instance.ShowResults();

        // Assert
        Assert.True(component.Instance.showResults);
        Assert.Equal("", component.Instance.statusMessage);
    }
}