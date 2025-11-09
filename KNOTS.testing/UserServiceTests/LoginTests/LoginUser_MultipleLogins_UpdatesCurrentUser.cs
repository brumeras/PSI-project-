namespace KNOTS.testing.UserServiceTests.LoginTests;

/// <summary>
/// Tests that logging in with a different user updates the CurrentUser property.
/// Verifies that the system correctly switches between logged-in users.
/// </summary>
public class LoginUser_MultipleLogins_UpdatesCurrentUser : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        UserService.RegisterUser("user1", "password123");
        UserService.RegisterUser("user2", "password456");

        // Act
        UserService.LoginUser("user1", "password123");
        Assert.Equal("user1", UserService.CurrentUser);

        UserService.LoginUser("user2", "password456");

        // Assert
        Assert.Equal("user2", UserService.CurrentUser);
    }
}