namespace TestProject1.PagesTests.NewGameTests;
using KNOTS.Services;
using KNOTS.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Moq;
using Xunit;
using KNOTS.Services.Interfaces;
using KNOTS.Models;

public class NewGame_CompletedStateTests
{
    [Fact]
    public void Shows_Completed_When_All_Statements_Answered()
    {
        using var ctx = new TestContext();
    
        var userMock = new Mock<InterfaceUserService>();
        userMock.Setup(x => x.IsAuthenticated).Returns(true);
        userMock.Setup(x => x.CurrentUser).Returns("Player1");
    
        var compsMock = new Mock<InterfaceCompatibilityService>();
        compsMock.Setup(x => x.GetRoomStatements(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<int>()))
            .Returns(new List<GameStatement>
            {
                new GameStatement { Id = "1", Text = "Test A", Topic = "SomeTopic" }
            });
        compsMock.Setup(x => x.SaveSwipe(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(true);
    
        ctx.Services.AddSingleton(userMock.Object);
        ctx.Services.AddSingleton(compsMock.Object);
        ctx.Services.AddSingleton(Mock.Of<IGameRoomService>());
    
        var cut = ctx.Render<NewGame>(p => p.Add(x => x.RoomCode, "123"));
    
        var swipeButton = cut.Find("button.swipe-btn.agree");
        swipeButton.Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("All Done!", cut.Markup);
        });

    
        // Wait for re-render and check if completed
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("All Done!", cut.Markup);
        }, timeout: TimeSpan.FromSeconds(5));
    }
}