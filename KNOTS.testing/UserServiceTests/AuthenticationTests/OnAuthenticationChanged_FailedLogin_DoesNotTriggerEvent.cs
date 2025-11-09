namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that OnAuthenticationChanged event does NOT fire when login fails.
/// Verifies subscribers are only notified on successful authentication state changes.
/// </summary>
public class OnAuthenticationChanged_FailedLogin_DoesNotTriggerEvent : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        
        var eventTriggered = false;
        UserService.OnAuthenticationChanged += () => eventTriggered = true;

        // Act
        UserService.LoginUser(username, "wrongpassword");

        // Assert
        Assert.False(eventTriggered);
    }
}