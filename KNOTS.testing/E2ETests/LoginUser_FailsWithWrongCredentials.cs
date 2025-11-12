using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class LoginUserFailsWithWrongCredentials : EndToEndTestBase
{
    [Fact]
    public void LoginUser_ReturnsFailureWhenPasswordIsIncorrect()
    {
        UserService.RegisterUser("login-user", "correct-password");

        var result = UserService.LoginUser("login-user", "wrong-password");

        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
        Assert.False(UserService.IsAuthenticated);
    }
}
