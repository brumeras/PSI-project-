using KNOTS.Components.Pages;
using KNOTS.Hubs;
using KNOTS.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.Hub_tests;

public class JoinRoomUnit {
    [Fact]
    public async Task JoinRoom(){
        var roomCode = "room1";
        var username = "pirmas";
        var connectionId = "con";

        var mockGameRoomService = new Mock<IGameRoomService>();
        var mockClients = new Mock<IHubCallerClients>();
        var mockCaller = new Mock<ISingleClientProxy>();
        var mockOthers = new Mock<IClientProxy>();
        var mockContext = new Mock<HubCallerContext>();

        mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        mockGameRoomService.Setup(s => s.JoinRoom(roomCode, connectionId, username)).Returns(new JoinRoomResult { Success = true });
        mockGameRoomService.Setup(s => s.GetRoomInfo(roomCode)).Returns(new GameRoom{
                RoomCode = roomCode,
                Players = new List<GamePlayer> {new GamePlayer(connectionId, username)}
            });

        mockClients.Setup(c => c.Caller).Returns(mockCaller.Object);
        mockClients.Setup(c => c.OthersInGroup(roomCode)).Returns(mockOthers.Object);
        mockCaller.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockOthers.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var mockGroups = new Mock<IGroupManager>();
        mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var hub = new GameHub(mockGameRoomService.Object) {
            Clients = mockClients.Object,
            Context = mockContext.Object,
            Groups = mockGroups.Object
        };
        await hub.JoinRoom(roomCode, username);
    }

}