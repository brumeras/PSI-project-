using Xunit;
using System;
using Bunit;
using KNOTS.Components.Pages;
using NSubstitute;

public class LoginPage_LoginButtonSuccessTests : LoginPageTestsBase
{
    [Fact]
    public void Login_Button_Calls_UserService_And_Navigates_On_Success()
    {
        _userServiceMock.IsAuthenticated.Returns(false);
        _userServiceMock.LoginUser(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, "OK"));

        var cut = Render<Login>();

        cut.Find("#usernameInput").Change("john");
        cut.Find("#passwordInput").Change("pass");

        cut.Find("button.btn-login").Click();

        _userServiceMock.Received(1).LoginUser("john", "pass");

        var navigated = ((TestNavigationManager)_navManager).Uri;
        Assert.Equal("/Home", new Uri(navigated).AbsolutePath);
    }
}