using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class CanJoinIfFullUnit : GameRoomBase {
    [Fact]
    public void CanJoin_ReturnsFailure_IfRoomIsFull() {
        var room = NewRoom(max: 1);
        room.Players.Add(Player("A", "vardas"));
        var result = room.CanJoin("username");
        Assert.False(result.Success);
        Assert.Equal("Room is full", result.Message);
        Assert.Equal(GameState.InProgress, result.State);
    }
}