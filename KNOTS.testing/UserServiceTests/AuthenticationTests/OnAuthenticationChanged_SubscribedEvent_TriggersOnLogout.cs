namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that OnAuthenticationChanged event fires when a user logs out.
/// Verifies event subscribers are notified when authentication state changes to logged out.
/// </summary>
public class OnAuthenticationChanged_SubscribedEvent_TriggersOnLogout : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        UserService.LoginUser(username, password);
        
        var eventCount = 0;
        UserService.OnAuthenticationChanged += () => eventCount++;

        // Act
        UserService.LogoutUser();

        // Assert
        Assert.Equal(1, eventCount);
    }
}