namespace KNOTS.testing.UserServiceTests.LoginTests;

/// <summary>
/// Tests that the OnAuthenticationChanged event is triggered after successful login.
/// Ensures subscribers are notified when authentication state changes.
/// </summary>
public class LoginUser_TriggersAuthenticationChangedEvent : UserServiceTestBase
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
        UserService.LoginUser(username, password);

        // Assert
        Assert.True(eventTriggered);
    }
}