using Xunit;

namespace KNOTS.testing.UserServiceTests.AuthenticationTests;

/// <summary>
/// Tests that IsAuthenticated is false and CurrentUser is null when UserService is created.
/// Verifies the initial authentication state before any login attempts.
/// </summary>
public class IsAuthenticated_InitialState_ReturnsFalse : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        Assert.False(UserService.IsAuthenticated); 
        Assert.Null(UserService.CurrentUser);
    }
}