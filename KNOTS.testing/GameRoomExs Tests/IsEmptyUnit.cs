using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class IsEmptyUnit : GameRoomBase {
    [Fact]
    public void IsEmptyReturnsTrueWhenNoPlayers() {
        var room = NewRoom();
        Assert.True(room.IsEmpty());
    }
}