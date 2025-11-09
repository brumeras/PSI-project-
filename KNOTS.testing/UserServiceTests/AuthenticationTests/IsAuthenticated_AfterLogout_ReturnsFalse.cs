namespace KNOTS.testing.UserServiceTests.AuthenticationTests;
  /// <summary>
    /// Tests that IsAuthenticated becomes false and CurrentUser is cleared after logout.
    /// Verifies the authentication state is properly reset when logging out.
    /// </summary>

public class IsAuthenticated_AfterLogout_ReturnsFalse: UserServiceTestBase
{
  
    [Fact]
    public void Test() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        UserService.LoginUser(username, password);

        // Act
        UserService.LogoutUser();

        // Assert
        Assert.False(UserService.IsAuthenticated);
        Assert.Null(UserService.CurrentUser);
    }
}