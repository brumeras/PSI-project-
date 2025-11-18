using System;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using KNOTS.Services.Interfaces;
using KNOTS.Components.Pages;
using KNOTS.Services;

public abstract class LoginPageTestsBase : TestContext
{
    protected readonly InterfaceUserService _userServiceMock;
    protected readonly NavigationManager _navManager;

    protected LoginPageTestsBase()
    {
        _userServiceMock = Substitute.For<InterfaceUserService>();
        Services.AddSingleton(_userServiceMock);

        Services.AddScoped<NavigationManager, TestNavigationManager>();
        _navManager = Services.GetRequiredService<NavigationManager>();
    }
}

public class TestNavigationManager : NavigationManager
{
    public TestNavigationManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        Uri = ToAbsoluteUri(uri).ToString();
    }
}