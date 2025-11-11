using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class AuthenticationChangedEventFiresOnLoginAndLogout : EndToEndTestBase
{
    [Fact]
    public void OnAuthenticationChanged_FiresForLoginAndLogout()
    {
        var triggerCount = 0;
        UserService.OnAuthenticationChanged += () => triggerCount++;

        UserService.RegisterUser("event-user", "event-pass");
        UserService.LoginUser("event-user", "event-pass");
        UserService.LogoutUser();

        Assert.Equal(2, triggerCount);
    }
}
