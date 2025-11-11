using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class UserRegistrationLoginLogoutCompletesFlow : EndToEndTestBase
{
    [Fact]
    public void UserRegistrationLoginLogout_CompletesEndToEndFlow()
    {
        var registerResult = UserService.RegisterUser("flow-user", "securePass1!");
        Assert.True(registerResult.Success);

        var loginResult = UserService.LoginUser("flow-user", "securePass1!");
        Assert.True(loginResult.Success);
        Assert.True(UserService.IsAuthenticated);
        Assert.Equal("flow-user", UserService.CurrentUser);

        UserService.LogoutUser();

        Assert.False(UserService.IsAuthenticated);
        Assert.Null(UserService.CurrentUser);
    }
}
