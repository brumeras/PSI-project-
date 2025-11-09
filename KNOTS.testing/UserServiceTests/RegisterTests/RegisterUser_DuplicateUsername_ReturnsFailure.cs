namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that registration fails when attempting to register with an existing username.
/// Ensures duplicate usernames are not allowed in the system.
/// </summary>
public class RegisterUser_DuplicateUsername_ReturnsFailure : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        var result = UserService.RegisterUser(username, "differentpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}