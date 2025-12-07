using KNOTS.Hubs;
using KNOTS.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.Hub_tests;

public class SendGameActionUnit{
    [Fact]
    public async Task SendGameAction(){
        var roomCode = "room1";
        var action = "mmm";
        var data = new { X = 1, Y = 2 };
        var connectionId = "con";
        var username = "pirmas";

        var mockGameRoomService = new Mock<IGameRoomService>();
        mockGameRoomService.Setup(s => s.GetPlayerUsername(connectionId)).Returns(username);
        var mockGroup = new Mock<IClientProxy>();
        mockGroup.Setup(g => g.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Group(roomCode)).Returns(mockGroup.Object);
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        var hub = new GameHub(mockGameRoomService.Object){
            Clients = mockClients.Object,
            Context = mockContext.Object
        };
        await hub.SendGameAction(roomCode, action, data);
        mockGroup.Verify(g => g.SendCoreAsync("GameAction", It.Is<object[]>(o => o.Length == 3 && (string)o[0] == username && (string)o[1] == action && o[2] == data), It.IsAny<CancellationToken>()), Times.Once);
    }
}
