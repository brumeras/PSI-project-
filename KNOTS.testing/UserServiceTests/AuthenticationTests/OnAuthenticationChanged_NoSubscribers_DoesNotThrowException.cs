namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that login succeeds even when no subscribers are attached to OnAuthenticationChanged.
/// Verifies the event is nullable and doesn't cause errors when no handlers are attached.
/// </summary>
public class OnAuthenticationChanged_NoSubscribers_DoesNotThrowException : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);

        // Act & Assert
        var exception = Record.Exception(() => UserService.LoginUser(username, password));
        Assert.Null(exception);
    }
}