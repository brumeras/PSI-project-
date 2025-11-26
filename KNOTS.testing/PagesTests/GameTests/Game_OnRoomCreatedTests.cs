using Bunit;
using KNOTS.Components.Pages;
using System.Threading.Tasks;

public class Game_OnRoomCreatedTests : GameTestBase
{
    [Fact]
    public async Task OnRoomCreated_ShouldUpdateRoomCodeAndStatus()
    {
        // Arrange
        var (component, _, _, _, _) = SetupGameComponent(currentUser: "Carol");

        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance. OnRoomCreated("ABCD");
        });

        // Assert
        Assert.Equal("ABCD", component.Instance.currentRoomCode);
        Assert.Contains("Room created! Code: ABCD", component. Markup);
        Assert.Contains("Carol", component.Markup);
    }
}