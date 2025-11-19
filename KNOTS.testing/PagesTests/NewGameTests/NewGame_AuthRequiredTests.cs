
namespace TestProject1.PagesTests.NewGameTests;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Components.Pages;


public class NewGame_AuthRequiredTests
{
    [Fact]
    public void Shows_Warning_When_Not_Authenticated()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(false);

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(Mock.Of<InterfaceCompatibilityService>());
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>();

        Assert.Contains("Please login to participate in the game.", cut.Markup);
    }
}
