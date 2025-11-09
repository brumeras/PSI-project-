namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that registration fails when password is shorter than 4 characters.
/// Verifies the error message indicates the minimum length requirement.
/// </summary>
public class RegisterUser_PasswordTooShort_ThrowsArgumentException : UserServiceTestBase
{
    [Theory]
    [InlineData("abc")]
    [InlineData("ab")]
    [InlineData("a")]
    public void Test(string password)
    {
        // Arrange
        var username = "testuser";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => UserService.RegisterUser(username, password));
        Assert.Contains("Password must be at least 4 characters long", exception.Message);
    }
}