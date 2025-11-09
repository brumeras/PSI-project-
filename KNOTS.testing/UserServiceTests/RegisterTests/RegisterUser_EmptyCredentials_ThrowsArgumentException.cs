namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that registration fails when username or password is empty or whitespace.
/// Should throw ArgumentException for any combination of empty/whitespace credentials.
/// </summary>
public class RegisterUser_EmptyCredentials_ThrowsArgumentException : UserServiceTestBase
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
        Assert.Throws<ArgumentException>(() => UserService.RegisterUser(username, password));
    }
}