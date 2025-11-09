namespace KNOTS.testing.UserServiceTests.LoginTests;

/// <summary>
/// Tests that login fails when an incorrect password is provided.
/// Verifies CurrentUser remains null and IsAuthenticated stays false.
/// </summary>
public class LoginUser_InvalidPassword_ReturnsFailure : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        var result = UserService.LoginUser(username, "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid username or password", result.Message);
        Assert.Null(UserService.CurrentUser);
        Assert.False(UserService.IsAuthenticated);
    }
}