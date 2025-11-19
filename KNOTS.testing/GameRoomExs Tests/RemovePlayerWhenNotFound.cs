namespace TestProject1.GameRoomExs_Tests;

public class RemovePlayerWhenNotFound : GameRoomBase {
    [Fact]
    public void RemovePlayerReturnsFalseIfPlayerNotFound() {
        var room = NewRoom();
        room.Players.Add(Player("1", "A"));
        var result = room.RemovePlayer("999");
        Assert.False(result);
        Assert.Single(room.Players);
    }
}