using KNOTS.Components;
using KNOTS.Services;
using KNOTS.Data;
using KNOTS.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=knots.db"));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CompatibilityService>();
builder.Services.AddScoped<LoggingService>();
builder.Services.AddSingleton<GameRoomService>();
builder.Services.AddSignalR();
var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<LoggingService>();
    try {
        dbContext.Database.EnsureCreated();
        var tableCount = dbContext.Model.GetEntityTypes().Count();
        var compatService = scope.ServiceProvider.GetRequiredService<CompatibilityService>();
    } catch (Exception ex){
        logger.LogException(ex, ex.Message);
    }
}

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHub<GameHub>("/gamehub");
app.Run();