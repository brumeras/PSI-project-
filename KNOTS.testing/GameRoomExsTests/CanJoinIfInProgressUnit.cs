using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class CanJoinIfInProgressUnit : GameRoomBase {
    [Fact]
    public void CanJoinReturnsFailIfGameStarted() {
        var room = NewRoom(state: GameState.InProgress);
        var result = room.CanJoin("username");
        Assert.False(result.Success);
        Assert.Equal("Game has already started", result.Message);
        Assert.Equal(GameState.InProgress, result.State);
    }
}