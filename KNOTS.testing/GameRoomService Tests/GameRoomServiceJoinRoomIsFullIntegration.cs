using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceJoinRoomIsFullIntegration {
    [Fact]
    public void JoinRoomSetStateToInProgressIfRoomIsFull() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmas");
        svc.JoinRoom(roomCode, "2", "antras");
        svc.JoinRoom(roomCode, "3", "trecias");
        svc.JoinRoom(roomCode, "4", "ketvirtas");
        var room = svc.GetRoomInfo(roomCode);
        Assert.Equal(GameState.InProgress, room!.State);
    }
}