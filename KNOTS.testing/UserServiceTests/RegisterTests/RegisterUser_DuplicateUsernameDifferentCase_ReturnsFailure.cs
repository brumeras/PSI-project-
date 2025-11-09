namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that username matching is case-insensitive during registration.
/// A user cannot register with "TestUser" if "TESTUSER" already exists.
/// </summary>
public class RegisterUser_DuplicateUsernameDifferentCase_ReturnsFailure : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "TestUser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        var result = UserService.RegisterUser("TESTUSER", "differentpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}