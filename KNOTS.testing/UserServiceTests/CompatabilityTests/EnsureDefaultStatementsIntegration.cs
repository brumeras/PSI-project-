using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class EnsureDefaultStatementsIntegration : UserServiceTestBase {
    [Fact]
    public void EnsureDefaultStatements_EnsureStatementsAdded() {
        Assert.Empty(Context.Statements);
        var compatService = new CompatibilityService(Context, UserService);
        Assert.Contains(Context.Statements, s => s.Id == "D1");
        Assert.Contains(Context.Statements, s => s.Id == "F2");
        Assert.Contains(Context.Statements, s => s.Id == "M3");
        Assert.Contains(Context.Statements, s => s.Id == "T4");
        Assert.Contains(Context.Statements, s => s.Id == "H5");

    }
}