using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceTransferHostIntegration {
    [Fact]
    public void RemovePlayerTransferHost() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmas");
        svc.JoinRoom(roomCode, "2", "antras");
        svc.RemovePlayer("1");
        var room = svc.GetRoomInfo(roomCode);
        Assert.Equal("antras", room!.Host);
    }
}