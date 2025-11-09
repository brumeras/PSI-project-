namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that CurrentUser remains null when login fails due to incorrect credentials.
/// Verifies the property is not modified on failed login attempts.
/// </summary>
public class CurrentUser_AfterFailedLogin_RemainsNull : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        UserService.LoginUser(username, "wrongpassword");

        // Assert
        Assert.Null(UserService.CurrentUser);
    }
}