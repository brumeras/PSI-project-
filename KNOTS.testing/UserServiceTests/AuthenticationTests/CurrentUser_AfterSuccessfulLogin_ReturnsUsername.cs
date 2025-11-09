using Xunit;

namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that CurrentUser property returns the username of the logged-in user.
/// Verifies the correct username is stored after successful authentication.
/// </summary>
public class CurrentUser_AfterSuccessfulLogin_ReturnsUsername : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        UserService.LoginUser(username, password);

        // Assert
        Assert.Equal(username, UserService.CurrentUser);
    }
}