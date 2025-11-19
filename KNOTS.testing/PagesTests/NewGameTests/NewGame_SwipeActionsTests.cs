using KNOTS.Components.Pages;
using KNOTS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.PagesTests.NewGameTests;

using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Models;

public class NewGame_SwipeActionsTests
{
    [Fact]
    public async Task SwipeRight_Saves_Answer_And_Advances()
    {
        using var ctx = new TestContext();

        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);
        userMock.Setup(x => x.CurrentUser).Returns("Tester");

        var statements = new List<GameStatement>
        {
            new GameStatement { Id = "A", Text = "Statement A" , Topic = "Test"},
            new GameStatement { Id = "B", Text = "Statement B" , Topic = "Test"}
        };

        var compMock = new Mock<InterfaceCompatibilityService>();
        compMock.Setup(x => x.GetRoomStatements("RC", It.IsAny<List<string>>(), 10))
            .Returns(statements);

        compMock.Setup(x => x.SaveSwipe("RC", "Tester", "A", true))
            .Returns(true);

        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());

        var cut = ctx.Render<NewGame>(p => p.Add(x => x.RoomCode, "RC"));

        var agreeBtn = cut.Find(".swipe-btn.agree");
        await agreeBtn.ClickAsync();

        compMock.Verify(x => x.SaveSwipe("RC", "Tester", "A", true), Times.Once);

        Assert.Contains("Statement 2 of 2", cut.Markup);
        Assert.Contains("Statement B", cut.Markup);
    }
}
