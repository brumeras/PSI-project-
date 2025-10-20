using KNOTS.Components;
using KNOTS.Services;
using KNOTS.Data;
using KNOTS.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SQLite duomenų bazė - SCOPED (saugus būdas)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=knots.db"));

// Servisai - visi Scoped (nes naudoja DbContext)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CompatibilityService>();

// GameRoomService - Singleton (neturi DB priklausomybių)
builder.Services.AddSingleton<GameRoomService>();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Sukuriame ir inicializuojame duomenų bazę
Console.WriteLine("🔧 Initializing database...");
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        // Development mode: ištrinti seną DB ir sukurti naują
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("🗑️  Deleting old database...");
            dbContext.Database.EnsureDeleted();
        }
        
        Console.WriteLine("📦 Creating database...");
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ Database created successfully");
        
        // Verify tables exist
        var tableCount = dbContext.Model.GetEntityTypes().Count();
        Console.WriteLine($"✅ Database has {tableCount} entity types configured");
        
        // Initialize CompatibilityService to create default statements
        var compatService = scope.ServiceProvider.GetRequiredService<CompatibilityService>();
        Console.WriteLine("✅ CompatibilityService initialized");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ⚠️ SVARBU: SignalR Hub mapping - BE ŠIO SignalR NEVEIKS!
app.MapHub<GameHub>("/gamehub");
Console.WriteLine("✅ SignalR Hub mapped to /gamehub");

Console.WriteLine("🚀 Application started successfully!");
Console.WriteLine("📍 Navigate to the application in your browser");

app.Run();