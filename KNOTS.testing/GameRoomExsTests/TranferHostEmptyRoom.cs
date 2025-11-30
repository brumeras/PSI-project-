using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class TranferHostEmptyRoom : GameRoomBase {
    [Fact]
    public void TransferHostWhenRoomIsEmpty() {
        var room = NewRoom();
        room.Host = "ExistingHost";
        room.TransferHost();
        Assert.Equal("ExistingHost", room.Host);
    }
}