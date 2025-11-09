namespace KNOTS.testing.UserServiceTests.LoginTests;

/// <summary>
/// Tests that login throws ArgumentException when credentials are empty or whitespace.
/// Validates all combinations of empty username/password inputs.
/// </summary>
public class LoginUser_EmptyCredentials_ThrowsArgumentException : UserServiceTestBase
{
    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    [InlineData("", "")]
    [InlineData("   ", "password")]
    [InlineData("username", "   ")]
    public void Test(string username, string password)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserService.LoginUser(username, password));
    }
}