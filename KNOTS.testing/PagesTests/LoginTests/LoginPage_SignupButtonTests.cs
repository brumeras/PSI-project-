using Xunit;
using System;
using Bunit;
using KNOTS.Components.Pages;
using NSubstitute;

public class LoginPage_SignupButtonTests : LoginPageTestsBase
{
    [Fact]
    public void Signup_Button_Calls_Register_And_Login_Then_Navigates_On_Success()
    {
        _userServiceMock.IsAuthenticated.Returns(false);

        _userServiceMock.RegisterUser(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, "Account created"));

        _userServiceMock.LoginUser(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, "OK"));

        var cut = Render<Login>();

        cut.Find("#usernameInput").Change("newuser");
        cut.Find("#passwordInput").Change("secret");

        cut.Find("button.btn-signup").Click();

        _userServiceMock.Received(1).RegisterUser("newuser", "secret");
        _userServiceMock.Received(1).LoginUser("newuser", "secret");

        var navigated = ((TestNavigationManager)_navManager).Uri;
        Assert.Equal("/Home", new Uri(navigated).AbsolutePath);
    }
}