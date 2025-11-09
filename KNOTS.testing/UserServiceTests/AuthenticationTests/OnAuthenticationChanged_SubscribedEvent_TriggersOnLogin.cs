namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that OnAuthenticationChanged event fires when a user logs in successfully.
/// Verifies event subscribers are notified of authentication state changes.
/// </summary>
public class OnAuthenticationChanged_SubscribedEvent_TriggersOnLogin : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        
        var eventCount = 0;
        UserService.OnAuthenticationChanged += () => eventCount++;

        // Act
        UserService.LoginUser(username, password);

        // Assert
        Assert.Equal(1, eventCount);
    }
}