using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that a compatibility score of 0.0 is handled correctly.
/// Verifies the system can process edge case scores without errors.
/// </summary>
public class UpdateUserStatistics_ZeroCompatibilityScore_UpdatesCorrectly : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        UserService.RegisterUser(username, "password123");

        // Act
        UserService.UpdateUserStatistics(username, 0.0, wasBestMatch: false);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(1, user.TotalGamesPlayed);
        Assert.Equal(0.0, user.AverageCompatibilityScore);
    }
}