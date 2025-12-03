using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceGetPlayerUsernameIntegration {
    [Fact]
    public void GetPlayerUsername() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmas");
        svc.JoinRoom(roomCode, "2", "antras");
        Assert.Equal("antras", svc.GetPlayerUsername("2"));
    }
}