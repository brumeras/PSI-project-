namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that passwords are hashed before being stored in the database.
/// Verifies the stored password hash is not equal to the plain text password.
/// </summary>
public class RegisterUser_PasswordIsHashed_NotStoredInPlainText : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";

        // Act
        UserService.RegisterUser(username, password);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);
    }
}