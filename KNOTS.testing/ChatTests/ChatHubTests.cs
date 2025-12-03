using System.Collections.Concurrent;
using System.Reflection;
using KNOTS.Hubs;
using KNOTS.Models;
using KNOTS.Services.Chat;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.ChatTests;

public class ChatHubTests {
    private ChatHub CreateHub(
        out Mock<IMessageService> messageServiceMock,
        out Mock<HubCallerContext> contextMock,
        out Mock<IHubCallerClients> clientsMock,
        out Mock<IGroupManager> groupsMock,
        out Mock<ISingleClientProxy> callerProxyMock,
        string connectionId = "conn-1")
    {
        messageServiceMock = new Mock<IMessageService>();
        clientsMock = new Mock<IHubCallerClients>();
        groupsMock = new Mock<IGroupManager>();
        callerProxyMock = new Mock<ISingleClientProxy>();
        clientsMock.Setup(c => c.Caller).Returns(callerProxyMock.Object);
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(callerProxyMock.Object);
        contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        var hub = new ChatHub(messageServiceMock.Object) {
            Context = contextMock.Object,
            Clients = clientsMock.Object,
            Groups = groupsMock.Object
        };
        return hub;
    }
    [Fact]
    public async Task SetUsername_AddsUserToGroup() {
        var hub = CreateHub(
            out var messageServiceMock,
            out var contextMock,
            out var clientsMock,
            out var groupsMock,
            out var callerProxyMock);
        await hub.SetUsername("Alice");
        groupsMock.Verify(g => g.AddToGroupAsync("conn-1", "user:alice", It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task WhoAmI_ReturnsNormalizedUsername() {
        var hub = CreateHub(
            out var messageServiceMock,
            out var contextMock,
            out var clientsMock,
            out var groupsMock,
            out var callerProxyMock);
        await hub.SetUsername("Alice");
        var result = await hub.WhoAmI();
        Assert.Equal("alice", result);
    }
    [Fact]
    public async Task SendChatMessage_PersistsAndCallsService() {
        var hub = CreateHub(
            out var messageServiceMock,
            out var contextMock,
            out var clientsMock,
            out var groupsMock,
            out var callerProxyMock);
        await hub.SetUsername("Alice");
        await hub.SendChatMessage("Bob", "Hello!");
        messageServiceMock.Verify(m =>
                m.SendMessage(It.Is<Message>(msg =>
                    msg.SenderId == "alice" &&
                    msg.ReceiverId == "bob" &&
                    msg.Content == "Hello!")),
            Times.Once);
    }
    [Fact]
    public async Task SendChatMessage_WithoutUsername_SendsErrorToCaller() {
        var field = typeof(ChatHub).GetField("UserToConnections", BindingFlags.NonPublic | BindingFlags.Static)!;
        var dict = (ConcurrentDictionary<string, HashSet<string>>)field.GetValue(null)!;
        dict.Clear();
        var hub = CreateHub(
            out var messageServiceMock,
            out var contextMock,
            out var clientsMock,
            out var groupsMock,
            out var callerProxyMock);
        await hub.SendChatMessage("Bob", "Hello!");
        messageServiceMock.Verify(m => m.SendMessage(It.IsAny<Message>()), Times.Never);
        callerProxyMock.Verify(p =>
                p.SendCoreAsync(
                    "Error",
                    It.Is<object[]>(args => (string)args[0] ==
                                            "Username not set. Please set username before sending messages."),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task OnDisconnectedAsync_RemovesConnectionFromGroup() {
        var hub = CreateHub(
            out var messageServiceMock,
            out var contextMock,
            out var clientsMock,
            out var groupsMock,
            out var callerProxyMock);
        await hub.SetUsername("Alice");
        await hub.OnDisconnectedAsync(null);
        groupsMock.Verify(g =>
                g.RemoveFromGroupAsync("conn-1", "user:alice", It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task SendChatMessage_InvalidArgs_DoesNothing() {
        var hub = CreateHub(
            out var messageServiceMock,
            out var contextMock,
            out var clientsMock,
            out var groupsMock,
            out var callerProxyMock);
        await hub.SetUsername("Alice");
        await hub.SendChatMessage("", "");
        await hub.SendChatMessage(null!, "content");
        await hub.SendChatMessage("Bob", null!);
        messageServiceMock.Verify(m => m.SendMessage(It.IsAny<Message>()), Times.Never);
    }

}