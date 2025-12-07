using KNOTS.Data;
using KNOTS.Services;
using KNOTS.Services.Chat;
using KNOTS.Services.Compability;
using KNOTS.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.ProgramTests;

public class ServiceRegistrationTest {
    [Fact]
    public void Services_AreRegistered() {
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));
        services.AddScoped<InterfaceLoggingService, LoggingService>(sp => new LoggingService("logs"));
        services.AddScoped<InterfaceSwipeRepository, SwipeRepository>();
        services.AddScoped<InterfaceCompatibilityCalculator, CompatibilityCalculator>();
        services.AddScoped<InterfaceUserService, UserService>();
        services.AddScoped<InterfaceCompatibilityService, CompatibilityService>();
        services.AddSingleton<IGameRoomService, GameRoomService>();
        services.AddScoped<IMessageService, MessageService>();
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<InterfaceLoggingService>());
        Assert.NotNull(provider.GetService<InterfaceSwipeRepository>());
        Assert.NotNull(provider.GetService<InterfaceCompatibilityCalculator>());
        Assert.NotNull(provider.GetService<InterfaceUserService>());
        Assert.NotNull(provider.GetService<InterfaceCompatibilityService>());
        Assert.NotNull(provider.GetService<IGameRoomService>());
        Assert.NotNull(provider.GetService<IMessageService>());
    }
}