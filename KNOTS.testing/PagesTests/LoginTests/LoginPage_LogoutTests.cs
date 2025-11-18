using Bunit;
using KNOTS.Components.Pages;
using NSubstitute;
using Xunit;

public class LoginPage_LogoutTests : LoginPageTestsBase
{
    [Fact]
    public void Logout_Calls_UserService_And_Shows_Login_Form()
    {
        _userServiceMock.IsAuthenticated.Returns(true);
        _userServiceMock.CurrentUser.Returns("TestUser");

        _userServiceMock.When(x => x.LogoutUser()).Do(ci =>
        {
            _userServiceMock.IsAuthenticated.Returns(false);
            _userServiceMock.CurrentUser.Returns(string.Empty);
        });

        var cut = Render<Login>();

        Assert.Contains("Welcome, TestUser!", cut.Markup);

        cut.Find("button.btn-logout-full").Click();

        _userServiceMock.Received(1).LogoutUser();

        Assert.Contains("Username", cut.Markup);
        Assert.Contains("Password", cut.Markup);
    }
}