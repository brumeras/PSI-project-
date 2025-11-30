using KNOTS.Hubs;
using KNOTS.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.Hub_tests;

public class UploadGameActionsUnit{
    [Fact]
    public async Task UploadGameActions(){
        var connectionId = "CONN1";
        var username = "Player1";
        var actions = new List<GameActionData> {
            new GameActionData { RoomCode = "ROOM1", Action = "Move", Data = new { X = 1 } },
            new GameActionData { RoomCode = "ROOM2", Action = "Jump", Data = new { Y = 2 } }
        };

        var mockGameRoomService = new Mock<IGameRoomService>();
        mockGameRoomService.Setup(s => s.GetPlayerUsername(connectionId)).Returns(username);
        var mockGroup1 = new Mock<IClientProxy>();
        mockGroup1.Setup(g => g.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var mockGroup2 = new Mock<IClientProxy>();
        mockGroup2.Setup(g => g.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Group("ROOM1")).Returns(mockGroup1.Object);
        mockClients.Setup(c => c.Group("ROOM2")).Returns(mockGroup2.Object);
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        
        var hub = new GameHub(mockGameRoomService.Object) {
            Clients = mockClients.Object,
            Context = mockContext.Object
        };
        await hub.UploadGameActions(actions.ToAsyncEnumerable());
        mockGroup1.Verify(g => g.SendCoreAsync("GameAction", It.Is<object[]>(o => (string)o[0] == username && (string)o[1] == "Move"), It.IsAny<CancellationToken>()), Times.Once);
        mockGroup2.Verify(g => g.SendCoreAsync("GameAction", It.Is<object[]>(o => (string)o[0] == username && (string)o[1] == "Jump"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
