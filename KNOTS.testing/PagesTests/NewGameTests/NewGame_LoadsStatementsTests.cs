using KNOTS.Services;
using KNOTS.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Models;

namespace TestProject1.PagesTests.NewGameTests;

public class NewGame_LoadsStatementsTests
{
    [Fact]
    public void Loads_And_Shows_First_Statement()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);
        userMock.Setup(x => x.CurrentUser).Returns("Player1");

        var statements = new List<GameStatement>
        {
            new GameStatement { Id = "1", Text = "First Statement", Topic = "SomeTopic" }
        };

        var compMock = new Mock<InterfaceCompatibilityService>();
        compMock.Setup(x => x.GetRoomStatements("ROOMX", It.IsAny<List<string>>(), It.IsAny<int>()))
            .Returns(statements);

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>(p =>
        {
            p.Add(x => x.RoomCode, "ROOMX");
        });

        Assert.Contains("First Statement", cut.Markup);
        Assert.Contains("Statement 1 of 1", cut.Markup);
    }
}