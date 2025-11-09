namespace KNOTS.testing.UserServiceTests.LogoutTests;

public class LogoutUser_MultipleLogouts_DoesNotThrowException : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        UserService.RegisterUser(username, password);
        UserService.LoginUser(username, password);

        // Act & Assert
        UserService.LogoutUser();
        var exception = Record.Exception(() => UserService.LogoutUser());
        Assert.Null(exception);
        Assert.False(UserService.IsAuthenticated);
    }
}