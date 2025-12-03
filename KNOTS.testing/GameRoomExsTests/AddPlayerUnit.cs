namespace TestProject1.GameRoomExs_Tests;

public class AddPlayerUnit : GameRoomBase{
    [Fact]
    public void AddPlayer() {
        var room = NewRoom();
        var p = Player("1", "Camila");
        room.AddPlayer(p);
        Assert.Single(room.Players);
        Assert.Equal("Camila", room.Players[0].Username);
    }
}