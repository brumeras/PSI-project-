namespace KNOTS.testing.UserServiceTests.LoginTests;

/// <summary>
/// Tests that a user can successfully log in with correct credentials.
/// Verifies CurrentUser is set and IsAuthenticated becomes true.
/// </summary>
public class LoginUser_ValidCredentials_ReturnsSuccess : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        var result = UserService.LoginUser(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.Equal(username, UserService.CurrentUser);
        Assert.True(UserService.IsAuthenticated);
    }
}