using Bunit;
using KNOTS.Components.Pages;
using System.Collections.Generic;
using Xunit;

public class Game_HandleResultsSavedTests : GameTestBase
{
    [Fact]
    public void HandleResultsSaved_ResetsStateAndShowsMessage()
    {
        // Arrange
        var (component, _, _, _, _) = SetupGameComponent();

        // Simulate being in a room
        var roomInfo = new Game.RoomInfo 
        { 
            RoomCode = "ROOM2", 
            Players = new List<string> { "Alice", "Ivy" } 
        };

        // Set up initial state
        component.Instance.currentRoomCode = roomInfo.RoomCode;
        component.Instance.playersInRoom.AddRange(roomInfo.Players);
        component.Instance.showResults = true;

        // Act
        component.InvokeAsync(() => component.Instance.HandleResultsSaved());

        // Assert
        Assert.Equal("", component.Instance.currentRoomCode);
        Assert.Empty(component.Instance.playersInRoom);
        Assert.False(component.Instance.showResults);
        Assert.Contains("Results saved", component.Markup);
    }
}