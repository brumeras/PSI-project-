using Bunit;
using KNOTS.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using Microsoft.JSInterop;

public class GameTestBase : TestContext
{
    protected (IRenderedComponent<Game>, Mock<NavigationManager>, Mock<InterfaceUserService>, Mock<InterfaceCompatibilityService>, Mock<IGameRoomService>) SetupGameComponent(
        bool isAuthenticated = true, 
        string currentUser = "TestUser")
    {
        var mockNav = new Mock<NavigationManager>();
        var mockUserService = new Mock<InterfaceUserService>();
        var mockCompatibility = new Mock<InterfaceCompatibilityService>();
        var mockGameRoom = new Mock<IGameRoomService>();
        var mockJSRuntime = new Mock<IJSRuntime>();

        mockUserService.Setup(u => u. IsAuthenticated).Returns(isAuthenticated);
        mockUserService.Setup(u => u.CurrentUser). Returns(currentUser);

        Services. AddSingleton(mockJSRuntime.Object);
        Services.AddSingleton(mockUserService.Object);
        Services. AddSingleton(mockCompatibility.Object);
        Services.AddSingleton(mockGameRoom. Object);
        Services.AddSingleton(mockNav.Object);

        var component = Render<Game>();

        return (component, mockNav, mockUserService, mockCompatibility, mockGameRoom);
    }
}