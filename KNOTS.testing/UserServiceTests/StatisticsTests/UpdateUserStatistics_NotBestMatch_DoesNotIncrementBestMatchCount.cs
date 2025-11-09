using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that BestMatchesCount is not incremented when wasBestMatch is false.
/// Verifies only TotalGamesPlayed and AverageCompatibilityScore are updated.
/// </summary>
public class UpdateUserStatistics_NotBestMatch_DoesNotIncrementBestMatchCount : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        UserService.RegisterUser(username, "password123");

        // Act
        UserService.UpdateUserStatistics(username, 50.0, wasBestMatch: false);
        UserService.UpdateUserStatistics(username, 60.0, wasBestMatch: false);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(2, user.TotalGamesPlayed);
        Assert.Equal(0, user.BestMatchesCount);
        Assert.Equal(55.0, user.AverageCompatibilityScore);
    }
}