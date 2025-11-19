using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceRemovePlayerIntegration{
    [Fact]
    public void RemovePlayer() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmase");
        svc.JoinRoom(roomCode, "2", "antras");
        var info = svc.RemovePlayer("2");
        Assert.Equal("antras", info.Username);
        Assert.Equal(roomCode, info.RoomCode);
        var players = svc.GetRoomPlayerUsernames(roomCode);
        Assert.DoesNotContain("antras", players);
    }

}