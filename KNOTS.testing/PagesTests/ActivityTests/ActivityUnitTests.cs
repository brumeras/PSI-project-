using System.Reflection;
using Bunit;
using KNOTS.Compability;
using KNOTS.Components.Pages;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using KNOTS.Tests.Integration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace TestProject1.PagesTests.ActivityTests;

using Bunit;
using KNOTS.Components.Pages;
using Moq;
using Xunit;

public class ActivityTests : TestContext
{
    [Fact]
    public void LoadGameHistory_ShouldPopulateGameHistory()
    {
        // Arrange
        var mockUserService = new Mock<InterfaceUserService>();
        var mockCompatibilityService = new Mock<InterfaceCompatibilityService>();

        mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
        mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");

        mockCompatibilityService
            .Setup(x => x.GetPlayerHistory("TestUser"))
            .Returns(new List<GameHistoryEntry>
            {
                new GameHistoryEntry { RoomCode = "ABC123", BestMatchPercentage = 90 }
            });

        Services.AddSingleton(mockUserService.Object);
        Services.AddSingleton(mockCompatibilityService.Object);

        // Act
        var cut = Render<Activity>();

        // Wait for async OnInitializedAsync to finish and markup to update
        cut.WaitForAssertion(() => 
            Assert.Contains("ABC123", cut.Markup));
    }
    
    [Theory]
    [InlineData(85, "high")]
    [InlineData(70, "medium")]
    [InlineData(50, "low")]
    [InlineData(20, "verylow")]
    public void GetProgressClass_PrivateMethod_WorksCorrectly(double percentage, string expected)
    {
        var activity = new Activity();
        var method = typeof(Activity).GetMethod("GetProgressClass", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method.Invoke(activity, new object[] { percentage });
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(85, "high")]
    [InlineData(75, "medium")]
    [InlineData(50, "low")]
    [InlineData(30, "verylow")]
    public void GetBadgeClass_ReturnsCorrectClass(double percentage, string expected)
    {
        var activity = new Activity();
        var method = typeof(Activity).GetMethod("GetBadgeClass", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method.Invoke(activity, new object[] { percentage });
        Assert.Equal(expected, result);
    }
}
