using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceJoinRoomIntegration {
    [Fact]
    public void JoinRoomAddPlayerToRoom() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmas");
        var result = svc.JoinRoom(roomCode, "1", "antras");
        Assert.True(result.Success);
        var players = svc.GetRoomPlayerUsernames(roomCode);
        Assert.Contains("antras", players);
    }
}