using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that updating statistics for a user's first game sets all values correctly.
/// Verifies TotalGamesPlayed is 1, BestMatchesCount is incremented, and average score is set.
/// </summary>
public class UpdateUserStatistics_FirstGame_UpdatesCorrectly : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        UserService.RegisterUser(username, "password123");
        var compatibilityScore = 85.5;

        // Act
        UserService.UpdateUserStatistics(username, compatibilityScore, wasBestMatch: true);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(1, user.TotalGamesPlayed);
        Assert.Equal(1, user.BestMatchesCount);
        Assert.Equal(85.5, user.AverageCompatibilityScore);
    }
}