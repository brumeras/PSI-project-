namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that all subscribers to OnAuthenticationChanged are notified when the event fires.
/// Verifies the event system supports multiple subscribers correctly.
/// </summary>
public class OnAuthenticationChanged_MultipleSubscribers_AllGetNotified : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        
        var subscriber1Called = false;
        var subscriber2Called = false;
        UserService.OnAuthenticationChanged += () => subscriber1Called = true;
        UserService.OnAuthenticationChanged += () => subscriber2Called = true;

        // Act
        UserService.LoginUser(username, password);

        // Assert
        Assert.True(subscriber1Called);
        Assert.True(subscriber2Called);
    }
}