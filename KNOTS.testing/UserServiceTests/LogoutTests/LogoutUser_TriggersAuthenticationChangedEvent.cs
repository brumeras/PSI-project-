namespace KNOTS.testing.UserServiceTests.LogoutTests;

/// <summary>
/// Tests that the OnAuthenticationChanged event is triggered when a user logs out.
/// Ensures subscribers are notified of the authentication state change.
/// </summary>
public class LogoutUser_TriggersAuthenticationChangedEvent : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        UserService.LoginUser(username, password);
        
        var eventTriggered = false;
        UserService.OnAuthenticationChanged += () => eventTriggered = true;

        // Act
        UserService.LogoutUser();

        // Assert
        Assert.True(eventTriggered);
    }
}