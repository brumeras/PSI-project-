using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceCreateRoomIntegration {
    [Fact]
    public void CreateRoom() {
        var svc = new GameRoomService();
        var roomCode = svc.CreateRoom("1", "pirmas");
        Assert.False(string.IsNullOrWhiteSpace(roomCode));
        var room = svc.GetRoomInfo(roomCode);
        Assert.NotNull(room);
        Assert.Equal("pirmas", room!.Host);
        Assert.Single(room.Players);
        Assert.Equal("pirmas", room.Players[0].Username);
    }
}