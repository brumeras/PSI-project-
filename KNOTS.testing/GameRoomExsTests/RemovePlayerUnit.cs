using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class RemovePlayerUnit : GameRoomBase {
    [Fact]
    public void RemovePlayerExisting() {
        var room = NewRoom();
        room.Players.Add(Player("10", "A"));
        var result = room.RemovePlayer("10");
        Assert.True(result);
        Assert.Empty(room.Players);
    }
}