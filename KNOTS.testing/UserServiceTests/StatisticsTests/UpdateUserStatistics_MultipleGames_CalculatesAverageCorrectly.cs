using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that the average compatibility score is calculated correctly over multiple games.
/// Verifies the running average formula: ((old_avg * (n-1)) + new_score) / n
/// </summary>
public class UpdateUserStatistics_MultipleGames_CalculatesAverageCorrectly : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        UserService.RegisterUser(username, "password123");

        // Act
        UserService.UpdateUserStatistics(username, 80.0, wasBestMatch: true);
        UserService.UpdateUserStatistics(username, 90.0, wasBestMatch: false);
        UserService.UpdateUserStatistics(username, 70.0, wasBestMatch: true);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(3, user.TotalGamesPlayed);
        Assert.Equal(2, user.BestMatchesCount);
        Assert.Equal(80.0, user.AverageCompatibilityScore, precision: 2);
    }
}