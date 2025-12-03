using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class CanJoinUsernameTakenUnit : GameRoomBase{
    [Fact]
    public void CanJoinReturnsFailIfUsernameTaken() {
        var room = NewRoom();
        room.Players.Add(Player("1", "Camila"));
        var result = room.CanJoin("Camila");
        Assert.False(result.Success);
        Assert.Equal("Username is already taken", result.Message);
        Assert.Equal(GameState.WaitingForPlayers, result.State);
    }

}