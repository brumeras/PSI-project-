namespace KNOTS.testing.UserServiceTests.LoginTests;

/// <summary>
/// Tests that login is case-insensitive for usernames.
/// A user registered as "TestUser" can login with "TESTUSER".
/// </summary>
public class LoginUser_CaseInsensitiveUsername_ReturnsSuccess : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "TestUser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        var result = UserService.LoginUser("TESTUSER", password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.True(UserService.IsAuthenticated);
    }
}