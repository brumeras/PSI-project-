using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class AddPlayerSetsStateUnit : GameRoomBase {
    [Fact]
    public void AddPlayerSetsStateToInProgressIfRoomBecomesFull() {
        var room = NewRoom(max: 1);
        var p = Player("1", "A");
        room.AddPlayer(p);
        Assert.Equal(GameState.InProgress, room.State);
    }

}