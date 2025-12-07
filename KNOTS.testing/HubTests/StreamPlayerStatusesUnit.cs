using KNOTS.Hubs;
using KNOTS.Services;
using Moq;

namespace TestProject1.Hub_tests;

public class StreamPlayerStatusesUnit{
    [Fact]
    public async Task StreamPlayerStatuses(){
        var roomCode = "room1";
        var connectionId = "con";
        var playerUsername = "pirmas";
        var mockGameRoomService = new Mock<IGameRoomService>();
        mockGameRoomService.Setup(s => s.GetRoomInfo(roomCode)).Returns(new GameRoom {
                RoomCode = roomCode,
                Players = new List<GamePlayer> { new GamePlayer(connectionId, playerUsername)}
            });
        var hub = new GameHub(mockGameRoomService.Object);
        using var cts = new CancellationTokenSource(100); // cancel after 100ms
        var statuses = new List<PlayerStatus>();
        await foreach (var status in hub.StreamPlayerStatuses(roomCode, cts.Token)) {
            statuses.Add(status);
            break;
        }
        Assert.Single(statuses);
        Assert.Equal(playerUsername, statuses[0].Username);
        Assert.True(statuses[0].IsOnline);
        Assert.True((DateTime.UtcNow - statuses[0].Timestamp).TotalSeconds < 5); // timestamp is recent
    }
}