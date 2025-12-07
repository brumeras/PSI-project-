using KNOTS.Data;
using KNOTS.Hubs;
using KNOTS.Services;
using KNOTS.Services.Chat;
using KNOTS.Services.Compability;
using KNOTS.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.ProgramTests;

public class ServiceRegistrationTest
{
    [Fact]
    public void Services_AreRegistered()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        // Register EF Core InMemory
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Register SignalR (required for IHubContext<T>)
        services.AddSignalR();

        // Register Hub itself (so DI can create it)
        services.AddSingleton<ChatHub>();

        // Register application services
        services.AddScoped<InterfaceLoggingService, LoggingService>(sp => new LoggingService("logs"));
        services.AddScoped<InterfaceSwipeRepository, SwipeRepository>();
        services.AddScoped<InterfaceCompatibilityCalculator, CompatibilityCalculator>();
        services.AddScoped<InterfaceUserService, UserService>();
        services.AddScoped<InterfaceCompatibilityService, CompatibilityService>();
        services.AddSingleton<IGameRoomService, GameRoomService>();
        services.AddScoped<IMessageService, MessageService>();

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<InterfaceLoggingService>());
        Assert.NotNull(provider.GetService<InterfaceSwipeRepository>());
        Assert.NotNull(provider.GetService<InterfaceCompatibilityCalculator>());
        Assert.NotNull(provider.GetService<InterfaceUserService>());
        Assert.NotNull(provider.GetService<InterfaceCompatibilityService>());
        Assert.NotNull(provider.GetService<IGameRoomService>());
        Assert.NotNull(provider.GetService<IMessageService>());
        
        var hubContext = provider.GetService<IHubContext<ChatHub>>();
        Assert.NotNull(hubContext);
    }
}
