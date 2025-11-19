using KNOTS.Components.Pages;
using KNOTS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.PagesTests.NewGameTests;

using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Models;

public class NewGame_ProgressTests
{
    [Fact]
    public void ProgressBar_Updates_When_Advancing()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);

        var statements = new List<GameStatement>
        {
            new GameStatement { Id = "1", Text = "S1", Topic = "Test" },
            new GameStatement { Id = "2", Text = "S2", Topic = "Test" }
        };

        var compMock = new Mock<InterfaceCompatibilityService>();
        compMock.Setup(x => x.GetRoomStatements("ROOM", null, 10))
            .Returns(statements);

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>(p => p.Add(x => x.RoomCode, "ROOM"));

        Assert.Contains("0%", cut.Markup);

        cut.Find(".nav-btn.skip").Click();

        Assert.Contains("50%", cut.Markup);
    }
}
