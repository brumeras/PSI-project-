using KNOTS.Components.Pages;
using KNOTS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.PagesTests.NewGameTests;

using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Models;

public class NewGame_SkipAndBackTests
{
    [Fact]
    public void Skip_Moves_To_Next_Statement()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);

        var statements = new List<GameStatement>
        {
            new GameStatement { Id = "1", Text = "S1" , Topic = "Test"},
            new GameStatement { Id = "2", Text = "S2" , Topic = "Test"}
        };

        var compMock = new Mock<InterfaceCompatibilityService>();
        compMock.Setup(x => x.GetRoomStatements("ROOM1", null, 10))
                .Returns(statements);

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>(p => p.Add(x => x.RoomCode, "ROOM1"));

        cut.Find(".nav-btn.skip").Click();

        Assert.Contains("Statement 2 of 2", cut.Markup);
    }

    [Fact]
    public void Back_Returns_To_Previous_Statement()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);

        var statements = new List<GameStatement>
        {
            new GameStatement { Id = "1", Text = "S1", Topic = "Test" },
            new GameStatement { Id = "2", Text = "S2" , Topic = "Test"}
        };

        var compMock = new Mock<InterfaceCompatibilityService>();
        compMock.Setup(x => x.GetRoomStatements("ROOM1", null, 10))
                .Returns(statements);

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>(p => p.Add(x => x.RoomCode, "ROOM1"));

        cut.Find(".nav-btn.skip").Click();
        cut.Find(".nav-btn.prev").Click();

        Assert.Contains("Statement 1 of 2", cut.Markup);
    }
}
