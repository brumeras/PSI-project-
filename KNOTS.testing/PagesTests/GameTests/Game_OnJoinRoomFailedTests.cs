using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using KNOTS.Components.Pages;
using System.Threading.Tasks;

public class Game_OnJoinRoomFailedTests : GameTestBase
{
    [Fact]
    public async Task OnJoinRoomFailed_ShouldShowErrorStatus()
    {
        // Arrange
        var (component, _, _, _, _) = SetupGameComponent();

        // Act
        await component.Instance. OnJoinRoomFailed("Room not found");
        component.Render(); // Trigger re-render after async operation

        // Assert
        Assert.Contains("Error: Room not found", component.Markup);
        Assert.False(component.Instance.isConnecting);
    }
}