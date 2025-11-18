using Bunit;
using KNOTS.Components.Pages;
using NSubstitute;
using Xunit;

public class LoginPage_RendersLoginFormTests : LoginPageTestsBase
{
    [Fact]
    public void Renders_Login_Form_When_Not_Authenticated()
    {
        _userServiceMock.IsAuthenticated.Returns(false);

        var cut = Render<Login>();

        Assert.Contains("Username", cut.Markup);
        Assert.Contains("Password", cut.Markup);
        cut.Find("button.btn-login");
    }
}
