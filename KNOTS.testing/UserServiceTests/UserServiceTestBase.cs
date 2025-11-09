using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

public abstract class UserServiceTestBase : IDisposable {
    protected readonly AppDbContext Context;
    protected readonly LoggingService Logger;
    protected readonly UserService UserService;

    protected UserServiceTestBase() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        Context = new AppDbContext(options);
        Logger = new LoggingService();
        UserService = new UserService(Context, Logger);
    }

    public void Dispose() {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}