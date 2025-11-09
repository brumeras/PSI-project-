namespace KNOTS.testing.UserServiceTests.AuthenticationTests;
/// <summary>
/// Tests that IsAuthenticated becomes true and CurrentUser is set after successful login.
/// Verifies the authentication state changes correctly after login.
/// </summary>
public class IsAuthenticated_AfterLogin_ReturnsTrue : UserServiceTestBase
{
    public void Test () {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act
        UserService.LoginUser(username, password);

        // Assert
        Assert.True(UserService.IsAuthenticated);
        Assert.NotNull(UserService.CurrentUser);
    }
}