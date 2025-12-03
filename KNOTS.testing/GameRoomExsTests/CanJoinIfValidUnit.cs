using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class CanJoinIfValidUnit : GameRoomBase{
    [Fact]
    public void CanJoinIfValid() {
        var room = NewRoom();
        var result = room.CanJoin("naujas");
        Assert.True(result.Success);
        Assert.Equal(GameState.WaitingForPlayers, result.State);
        Assert.Null(result.Message);
    }
}