using Xunit;

namespace KNOTS.testing.UserServiceTests.StatisticsTests;

/// <summary>
/// Tests that GetTotalUsersCount returns the correct number of registered users.
/// Verifies the method accurately counts all users in the database.
/// </summary>
public class GetTotalUsersCount_MultipleUsers_ReturnsCorrectCount : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        UserService.RegisterUser("user1", "password123");
        UserService.RegisterUser("user2", "password456");
        UserService.RegisterUser("user3", "password789");

        // Act
        var count = UserService.GetTotalUsersCount();

        // Assert
        Assert.Equal(3, count);
    }
}