using KNOTS.Services;

namespace TestProject1.GameRoomService_Tests;

public class GameRoomServiceGetRoomInfo {
    [Fact]
    public void GetRoomInfo() {
        var svc = new GameRoomService();
        var code = svc.CreateRoom("1", "pirmas");
        var room = svc.GetRoomInfo(code);
        Assert.NotNull(room);
        Assert.Equal(code, room!.RoomCode);
        Assert.Equal("pirmas", room.Host);
    }
}
