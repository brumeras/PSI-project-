using KNOTS.Hubs;

namespace TestProject1.Hub_tests;

public class StreamRoomUpdatesUnit{
    [Fact]
    public async Task StreamRoomUpdates(){
        var hub = new GameHub(null!);
        var roomCode = "room1";
        using var cts = new CancellationTokenSource(50);
        var enumerator = hub.StreamRoomUpdates(roomCode, cts.Token).GetAsyncEnumerator();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => {
            while (await enumerator.MoveNextAsync()) {var update = enumerator.Current; }
        });
    }
}