using System;
using System.Linq;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class RegisterUserFailsWhenUsernameAlreadyExists : EndToEndTestBase
{
    [Fact]
    public void RegisterUser_ReturnsFailureWhenUsernameAlreadyExists()
    {
        var firstAttempt = UserService.RegisterUser("duplicate-user", "pass1234");
        Assert.True(firstAttempt.Success);

        var secondAttempt = UserService.RegisterUser("duplicate-user", "pass1234");

        Assert.False(secondAttempt.Success);
        Assert.Contains("already exists", secondAttempt.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, Context.Users.Count());
    }
}
