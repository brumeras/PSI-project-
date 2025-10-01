using KNOTS.Components;
using KNOTS.Services;
using KNOTS.Hubs;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Register UserDataStore as Singleton (shared data file)
builder.Services.AddSingleton<UserDataStore>();

// Register CircuitHandler and UserService as SCOPED (one per browser tab/circuit)
builder.Services.AddScoped<AuthenticationCircuitHandler>();
builder.Services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<AuthenticationCircuitHandler>());
builder.Services.AddScoped<UserService>();

// Register GameRoomService as SINGLETON
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

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<GameHub>("/gamehub");

app.Run();