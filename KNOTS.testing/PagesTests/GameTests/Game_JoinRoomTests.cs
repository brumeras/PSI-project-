using Bunit;
using KNOTS.Components.Pages;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Moq;

public class Game_JoinRoomTests : GameTestBase
{
    [Fact]
    public void ClickingJoinRoom_InvokesJSAndShowsStatus()
    {
        // Arrange
        var (component, _, _, _, _) = SetupGameComponent(currentUser: "Bob");

        var mockJSRuntime = Services.GetRequiredService<IJSRuntime>() as Mock<IJSRuntime>;
        mockJSRuntime?.Setup(js => js.InvokeVoidAsync("joinRoom", It.IsAny<object[]>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        component.Find(".room-code-input").Change("1234");
        component.Find(".btn-join").Click();

        // Assert
        Assert.Contains("Connecting to room 1234", component.Markup);
    }
}