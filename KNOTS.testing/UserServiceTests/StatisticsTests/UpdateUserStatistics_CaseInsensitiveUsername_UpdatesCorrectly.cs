using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that username matching is case-insensitive when updating statistics.
/// A user registered as "TestUser" can have stats updated using "TESTUSER".
/// </summary>
public class UpdateUserStatistics_CaseInsensitiveUsername_UpdatesCorrectly : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "TestUser";
        UserService.RegisterUser(username, "password123");

        // Act
        UserService.UpdateUserStatistics("TESTUSER", 75.0, wasBestMatch: true);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(1, user.TotalGamesPlayed);
        Assert.Equal(1, user.BestMatchesCount);
    }
}