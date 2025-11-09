using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that GetTotalUsersCount returns 0 when there are no users in the database.
/// Verifies the method handles an empty database correctly.
/// </summary>
public class GetTotalUsersCount_NoUsers_ReturnsZero : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Act
        var count = UserService.GetTotalUsersCount();

        // Assert
        Assert.Equal(0, count);
    }
}