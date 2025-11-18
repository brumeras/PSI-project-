using Bunit;
using KNOTS.Components.Pages;
using NSubstitute;
using Xunit;

public class LoginPage_RendersWelcomeTests : LoginPageTestsBase
{
    [Fact]
    public void Renders_Welcome_When_Authenticated()
    {
        _userServiceMock.IsAuthenticated.Returns(true);
        _userServiceMock.CurrentUser.Returns("TestUser");

        var cut = Render<Login>();

        Assert.Contains("Welcome, TestUser!", cut.Markup);
        cut.Find("button.btn-logout-full");
    }
}