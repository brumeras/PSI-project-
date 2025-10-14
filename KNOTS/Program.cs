using KNOTS.Components;
using KNOTS.Services;
using KNOTS.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SQLite duomenų bazės konfigūracija - SINGLETON!
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=knots.db"), 
    ServiceLifetime.Singleton); // <-- SVARBU!

// UserService kaip Singleton
builder.Services.AddSingleton<UserService>();

// Kitus servisus palikite kaip Scoped
builder.Services.AddScoped<CompatibilityService>();
builder.Services.AddScoped<GameRoomService>();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Sukuriame DB
var dbContext = app.Services.GetRequiredService<AppDbContext>();
dbContext.Database.EnsureCreated();
Console.WriteLine("✅ Database created/verified successfully");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();