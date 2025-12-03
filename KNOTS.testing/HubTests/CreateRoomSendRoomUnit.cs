using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.Hub_tests;

public class CreateRoomSendRoomUnit : GameHubTestBase {
    [Fact]
    public async Task CreateRoomSendRoom(){
        var username = "pirmas";
        var roomCode = "room1";
        MockGameRoomService.Setup(s => s.CreateRoom("test-connection", username)).Returns(roomCode);
        MockGroups.Setup(g => g.AddToGroupAsync("test-connection", roomCode, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        MockClientProxy.Setup(c => c.SendCoreAsync("RoomCreated", It.Is<object[]>(o => o.Length == 1 && (string)o[0] == roomCode), default)).Returns(Task.CompletedTask);
        await Hub.CreateRoom(username);
        MockGroups.Verify(g => g.AddToGroupAsync("test-connection", roomCode, It.IsAny<CancellationToken>()), Times.Once);
        MockClientProxy.Verify(x => x.SendCoreAsync("RoomCreated", It.Is<object[]>(o => o.Length == 1 && (string)o[0] == roomCode), default), Times.Once);
    }
}