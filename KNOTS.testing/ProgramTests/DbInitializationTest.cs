using KNOTS.Data;
using Microsoft.EntityFrameworkCore;

namespace TestProject1.ProgramTests;

public class DbInitializationTest {
    [Fact]
    public void DbContext_CreatesTables() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        using var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        var tableCount = context.Model.GetEntityTypes().Count();
        Assert.True(tableCount > 0);
    }
}