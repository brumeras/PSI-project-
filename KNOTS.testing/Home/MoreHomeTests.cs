using Bunit;
using Microsoft.Extensions.DependencyInjection;
using KNOTS.Services;
using Xunit;
using System;
using Bunit.TestDoubles;
using KNOTS.Components.Pages;

public class HomeTests
{
    private class FakeUserService : InterfaceUserService
    {
        public string? CurrentUser { get; private set; } = null;
        public bool IsAuthenticated { get; private set; } = false;

        public event Action? OnAuthenticationChanged;

        public (bool Success, string Message) RegisterUser(string username, string password)
            => (true, "ok");

        public (bool Success, string Message) LoginUser(string username, string password)
        {
            CurrentUser = username;
            IsAuthenticated = true;
            OnAuthenticationChanged?.Invoke();
            return (true, "ok");
        }

        public void LogoutUser()
        {
            CurrentUser = null;
            IsAuthenticated = false;
            OnAuthenticationChanged?.Invoke();
        }

        public int GetTotalUsersCount() => 1;

        public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch) { }

        public List<User> GetLeaderboard(int topCount = 10) => new();

        public int GetUserRank(string username) => 1;

        public void SetUser(string? name, bool auth)
        {
            CurrentUser = name;
            IsAuthenticated = auth;
        }
    }

    // -----------------------------------------------------------------------------------
    // TESTS
    // -----------------------------------------------------------------------------------

    [Fact]
    public void Home_WhenUserNotAuthenticated_ShowsNotLoggedInMessage()
    {
        using var ctx = new TestContext();

        var fake = new FakeUserService();
        fake.SetUser(null, false);

        ctx.Services.AddSingleton<InterfaceUserService>(fake);

        var cut = ctx.RenderComponent<Home>();

        cut.Markup.Contains("You are not logged in.");
        cut.Markup.Contains("Go to login page");
        cut.Markup.Contains("Login"); // indirectly
    }

    [Fact]
    public void Home_WhenUserAuthenticated_ShowsWelcomeText()
    {
        using var ctx = new TestContext();

        var fake = new FakeUserService();
        fake.SetUser("TestUser", true);

        ctx.Services.AddSingleton<InterfaceUserService>(fake);

        var cut = ctx.RenderComponent<Home>();

        Assert.Contains("Welcome, TestUser!", cut.Markup);
        Assert.Contains("Logout", cut.Markup);
        Assert.DoesNotContain("You are not logged in", cut.Markup);
    }

    [Fact]
    public void Home_WhenAuthenticated_ShowsNavigationLinks()
    {
        using var ctx = new TestContext();

        var fake = new FakeUserService();
        fake.SetUser("Tester", true);

        ctx.Services.AddSingleton<InterfaceUserService>(fake);

        var cut = ctx.RenderComponent<Home>();

        Assert.Contains("/Home", cut.Markup);
        Assert.Contains("/activity", cut.Markup);
        Assert.Contains("/leaderboard", cut.Markup);
    }

    [Fact]
    public void Home_LogoutButton_CallsLogoutUser_AndRedirects()
    {
        using var ctx = new TestContext();

        var fake = new FakeUserService();
        fake.SetUser("User", true);

        ctx.Services.AddSingleton<InterfaceUserService>(fake);

        var nav = ctx.Services.GetRequiredService<FakeNavigationManager>();

        var cut = ctx.RenderComponent<Home>();

        // Find and click logout button
        cut.Find("button.btn-logout").Click();

        Assert.False(fake.IsAuthenticated);
        Assert.Null(fake.CurrentUser);
        Assert.Equal("/", nav.Uri);  // redirect successful
    }
}
