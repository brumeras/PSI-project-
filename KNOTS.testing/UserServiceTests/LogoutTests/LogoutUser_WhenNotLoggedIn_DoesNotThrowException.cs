namespace KNOTS.testing.UserServiceTests.LogoutTests;

/// <summary>
/// Tests that calling LogoutUser when no user is logged in does not throw an exception.
/// Verifies the method handles the edge case gracefully.
/// </summary>
public class LogoutUser_WhenNotLoggedIn_DoesNotThrowException : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Act & Assert
        var exception = Record.Exception(() => UserService.LogoutUser());
        Assert.Null(exception);
        Assert.False(UserService.IsAuthenticated);
    }
}