namespace TestProject1.GameRoomExs_Tests;

public class IsEmptyReturnsFalseUnit : GameRoomBase{
    [Fact]
    public void IsEmptyReturnsFalseIfPlayersPresent(){
        var room = NewRoom();
        room.Players.Add(Player("1", "A"));
        Assert.False(room.IsEmpty());
    }
}