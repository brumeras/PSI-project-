using KNOTS.Components;
using KNOTS.Services;
using KNOTS.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Registruojam servisus
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameRoomService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<GameHub>("/gamehub");

app.Run();