using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that a perfect compatibility score of 100.0 is handled correctly.
/// Verifies the system can process maximum score values without errors.
/// </summary>
public class UpdateUserStatistics_HighCompatibilityScore_UpdatesCorrectly : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        UserService.RegisterUser(username, "password123");

        // Act
        UserService.UpdateUserStatistics(username, 100.0, wasBestMatch: true);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(100.0, user.AverageCompatibilityScore);
    }
}