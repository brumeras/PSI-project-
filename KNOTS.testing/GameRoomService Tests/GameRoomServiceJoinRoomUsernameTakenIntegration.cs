using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceJoinRoomUsernameTakenIntegration {
    [Fact]
    public void JoinRoomUsernameTaken() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmas");
        svc.JoinRoom(roomCode, "2", "antras");
        var result = svc.JoinRoom(roomCode, "3", "antras");
        Assert.False(result.Success);
        Assert.Equal("Username is already taken", result.Message);
    }
}