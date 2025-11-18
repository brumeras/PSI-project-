using Bunit;
using KNOTS.Components.Pages;
using NSubstitute;
using Xunit;

public class LoginPage_LoginButtonFailureTests : LoginPageTestsBase
{
    [Fact]
    public void Login_Button_Shows_StatusMessage_On_Failure()
    {
        _userServiceMock.IsAuthenticated.Returns(false);
        _userServiceMock.LoginUser(Arg.Any<string>(), Arg.Any<string>())
            .Returns((false, "Invalid credentials"));

        var cut = Render<Login>();

        cut.Find("#usernameInput").Change("bad");
        cut.Find("#passwordInput").Change("wrong");

        cut.Find("button.btn-login").Click();

        Assert.Contains("Invalid credentials", cut.Markup);
    }
}