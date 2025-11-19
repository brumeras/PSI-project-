using KNOTS.Components.Pages;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.PagesTests.NewGameTests;

using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Components;


public class NewGame_NoStatementsShowsLoadingTests
{
    [Fact]
    public void Shows_Loading_When_No_Statements()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);

        var compMock = new Mock<InterfaceCompatibilityService>();
        compMock.Setup(x => x.GetRoomStatements(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<int>()))
            .Returns(new List<KNOTS.Models.GameStatement>());

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>(p =>
        {
            p.Add(x => x.RoomCode, "ROOM1");
        });

        Assert.Contains("Loading game statements...", cut.Markup);
    }
}
