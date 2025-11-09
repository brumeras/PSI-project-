using Xunit;
using KNOTS.Exceptions;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that updating statistics for a non-existent user throws UserNotFoundException.
/// Verifies proper error handling for invalid usernames.
/// </summary>
public class UpdateUserStatistics_NonExistentUser_ThrowsUserNotFoundException : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Act & Assert
        Assert.Throws<UserNotFoundException>(() =>
            UserService.UpdateUserStatistics("nonexistent", 50.0, false));
    }
}