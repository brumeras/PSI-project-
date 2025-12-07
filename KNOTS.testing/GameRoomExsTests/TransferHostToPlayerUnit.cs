namespace TestProject1.GameRoomExs_Tests;

public class TransferHostToPlayerUnit : GameRoomBase {
    [Fact]
    public void TransferHostToFirstPlayerIfPlayersExist() {
        var room = NewRoom();
        room.Players.Add(Player("1", "pirmas"));
        room.Players.Add(Player("2", "antras"));
        room.TransferHost();
        Assert.Equal("pirmas", room.Host);
    }

}