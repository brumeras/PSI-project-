using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using KNOTS.Components.Pages;

public class Game_CreateRoomTests : GameTestBase
{
    [Fact]
    public void ClickingCreateRoom_InvokesJSAndShowsStatus()
    {
        // Arrange
        var (component, _, _, _, _) = SetupGameComponent();

        var mockJSRuntime = Services.GetRequiredService<IJSRuntime>() as Mock<IJSRuntime>;
        mockJSRuntime?. Setup(js => js.InvokeVoidAsync("createRoom", It.IsAny<object[]>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var button = component.Find(".btn-create");
        button.Click();

        // Assert
        Assert.Contains("Creating room", component. Markup);
    }
}