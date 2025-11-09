namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that the CreatedAt timestamp is set correctly during registration.
/// Verifies the timestamp falls within a reasonable time range around the registration.
/// </summary>
public class RegisterUser_SetsCreatedAtDate : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var beforeRegistration = DateTime.Now.AddSeconds(-1);

        // Act
        UserService.RegisterUser(username, password);
        var afterRegistration = DateTime.Now.AddSeconds(1);

        // Assert
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.InRange(user.CreatedAt, beforeRegistration, afterRegistration);
    }
}