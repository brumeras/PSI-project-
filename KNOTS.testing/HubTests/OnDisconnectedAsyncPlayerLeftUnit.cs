using KNOTS.Hubs;
using KNOTS.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.Hub_tests;

public class OnDisconnectedAsyncPlayerLeftUnit{
    [Fact]
    public async Task OnDisconnectedAsyncPlayerLeft(){
        var connectionId = "con";
        var roomCode = "room1";
        var username = "pirmas";
        var mockGameRoomService = new Mock<IGameRoomService>();
        mockGameRoomService.Setup(s => s.RemovePlayer(connectionId)).Returns(new DisconnectedPlayerInfo {
                RoomCode = roomCode,
                Username = username
            });
        var mockGroup = new Mock<IClientProxy>();
        mockGroup.Setup(g => g.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Group(roomCode)).Returns(mockGroup.Object);
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        var hub = new GameHub(mockGameRoomService.Object) {
            Clients = mockClients.Object,
            Context = mockContext.Object
        };
        await hub.OnDisconnectedAsync(null);
        mockGroup.Verify(g => g.SendCoreAsync("PlayerLeft", It.Is<object[]>(o => (string)o[0] == username), It.IsAny<CancellationToken>()), Times.Once);
    }
}