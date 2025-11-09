namespace KNOTS.testing.UserServiceTests.LogoutTests;

/// <summary>
/// Tests that logging out clears the CurrentUser and sets IsAuthenticated to false.
/// Verifies the user is properly logged out after being logged in.
/// </summary>
public class LogoutUser_WhenLoggedIn_ClearsCurrentUser : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        UserService.LoginUser(username, password);
        Assert.True(UserService.IsAuthenticated);

        // Act
        UserService.LogoutUser();

        // Assert
        Assert.Null(UserService.CurrentUser);
        Assert.False(UserService.IsAuthenticated);
    }
}