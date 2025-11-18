using KNOTS.Components;
using KNOTS.Services;
using KNOTS.Services.Interfaces;   
using KNOTS.Data;
using KNOTS.Hubs;
using KNOTS.Services.Compability;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite("Data Source=knots.db"));

builder.Services.AddScoped<InterfaceLoggingService, LoggingService>(sp => 
    new LoggingService("logs"));

builder.Services.AddScoped<InterfaceSwipeRepository, SwipeRepository>();

builder.Services.AddScoped<InterfaceCompatibilityCalculator, CompatibilityCalculator>();

builder.Services.AddScoped<InterfaceUserService, UserService>();

builder.Services.AddScoped<InterfaceCompatibilityService, CompatibilityService>();

builder.Services.AddSingleton<GameRoomService>();

builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
<<<<<<< HEAD
    var logger = scope.ServiceProvider.GetRequiredService<LoggingService>();
    try {
        dbContext.Database.EnsureCreated();
        var tableCount = dbContext.Model.GetEntityTypes().Count();
        var compatService = scope.ServiceProvider.GetRequiredService<CompatibilityService>();
    } catch (Exception ex){
        logger.LogException(ex, ex.Message);
=======
    try
    {
        dbContext.Database.EnsureCreated();
        var tableCount = dbContext.Model.GetEntityTypes().Count();
        Console.WriteLine($"Database created successfully with {tableCount} tables");
        
        var compatService = scope.ServiceProvider.GetRequiredService<InterfaceCompatibilityService>();
        Console.WriteLine("CompatibilityService initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
>>>>>>> DependencyInjection
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHub<GameHub>("/gamehub");

app.Run();