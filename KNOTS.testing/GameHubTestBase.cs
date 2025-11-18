using KNOTS.Hubs;
using KNOTS.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1;

public abstract class  GameHubTestBase{
    protected readonly Mock<IGameRoomService> MockGameRoomService;
    protected readonly Mock<IHubCallerClients> MockClients;
    protected readonly Mock<ISingleClientProxy> MockClientProxy;
    protected readonly Mock<IGroupManager> MockGroups;

    protected readonly GameHub Hub;

    protected GameHubTestBase()
    {
        // Mock service
        MockGameRoomService = new Mock<IGameRoomService>(MockBehavior.Strict);

        // Mock SignalR client proxies
        MockClients = new Mock<IHubCallerClients>(MockBehavior.Strict);
        MockClientProxy = new Mock<ISingleClientProxy>(MockBehavior.Strict);

        // Caller → always return our client proxy
        MockClients
            .Setup(c => c.Caller)
            .Returns(MockClientProxy.Object);

        // Groups mock
        MockGroups = new Mock<IGroupManager>(MockBehavior.Strict);

        // Create hub with mocked GameRoomService
        Hub = new GameHub(MockGameRoomService.Object)
        {
            Clients = MockClients.Object,
            Groups = MockGroups.Object
        };

        // Set fake connection context
        SetConnectionId("test-connection");
    }

    /// <summary>
    /// Sets a fake HubCallerContext with a connection ID.
    /// </summary>
    protected void SetConnectionId(string connectionId)
    {
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        Hub.Context = mockContext.Object;
    }
}