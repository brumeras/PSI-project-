using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Compability;
using Microsoft.EntityFrameworkCore;

namespace KNOTS.Testing;

/// <summary>
/// Base class for end-to-end (E2E) tests that sets up an in-memory
/// environment with all core services and dependencies.
/// </summary>
/// <remarks>
/// This allows simulating complete gameplay scenarios — from user registration
/// to swiping and compatibility calculations — without touching a real database.
/// </remarks>
public abstract class EndToEndTestBase : IDisposable
{
    /// <summary>
    /// The in-memory EF Core database context used for testing.
    /// </summary>
    protected readonly AppDbContext Context;

    /// <summary>
    /// Common logging service shared across tests.
    /// </summary>
    protected readonly LoggingService Logger;

    /// <summary>
    /// Provides user authentication, registration, and stats management.
    /// </summary>
    protected readonly UserService UserService;

    /// <summary>
    /// Provides compatibility calculations and swipe logic.
    /// </summary>
    protected readonly CompatibilityService CompatibilityService;

    /// <summary>
    /// Provides direct access to player swipe data.
    /// </summary>
    protected readonly SwipeRepository SwipeRepository;

    /// <summary>
    /// Provides access to the compatibility calculator logic.
    /// </summary>
    protected readonly CompatibilityCalculator Calculator;

    /// <summary>
    /// Initializes a full in-memory test environment with services.
    /// </summary>
    protected EndToEndTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB for isolation
            .Options;

        Context = new AppDbContext(options);
        Logger = new LoggingService();

        // Initialize repositories and services (in realistic order)
        SwipeRepository = new SwipeRepository(Context);
        Calculator = new CompatibilityCalculator(SwipeRepository);
        UserService = new UserService(Context, Logger);
        CompatibilityService = new CompatibilityService(Context, UserService);

        Console.WriteLine("[E2E Setup] In-memory environment initialized.");
    }

    /// <summary>
    /// Helper to quickly register multiple users for testing.
    /// </summary>
    protected void RegisterTestUsers(params string[] usernames)
    {
        foreach (var username in usernames)
        {
            var result = UserService.RegisterUser(username, "testpass");
            if (!result.Success)
                Console.WriteLine($"⚠️ Failed to register {username}: {result.Message}");
        }

        Console.WriteLine($"✅ Registered {usernames.Length} test users.");
    }

    /// <summary>
    /// Cleans up the in-memory database and disposes of resources.
    /// </summary>
    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        Console.WriteLine("[E2E Teardown] Test environment disposed.");
    }

    /// <summary>
    /// Clears all statements from the in-memory database and seeds the provided set.
    /// </summary>
    /// <param name="statements">Statements that should exist after seeding.</param>
    protected void SeedStatements(IEnumerable<GameStatement> statements)
    {
        Context.Statements.RemoveRange(Context.Statements);
        Context.SaveChanges();

        Context.Statements.AddRange(statements);
        Context.SaveChanges();

        ResetRoomStatementCache();
    }

    /// <summary>
    /// Resets the static room statement cache on the CompatibilityService to ensure
    /// deterministic behaviour between tests.
    /// </summary>
    protected void ResetRoomStatementCache()
    {
        var cacheField = typeof(CompatibilityService)
            .GetField("_roomStatements", BindingFlags.NonPublic | BindingFlags.Static);

        cacheField?.SetValue(null, new ConcurrentDictionary<string, List<GameStatement>>());
    }
}