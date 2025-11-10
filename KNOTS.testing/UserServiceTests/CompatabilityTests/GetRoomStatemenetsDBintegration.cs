using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetRoomStatemenetsDBintegration : UserServiceTestBase {
    [Fact]
    public void GetRoomStatemenets_EnsuresReadFromDB() {
        var service = new CompatibilityService(Context, UserService);
        var res = service.GetRoomStatements("room1", null, 10);
        Assert.NotEmpty(res);
        Assert.True(Context.Statements.Any());
        Assert.All(res, s => Assert.Contains(s.Id, Context.Statements.Select(x => x.Id)));
    }
} 