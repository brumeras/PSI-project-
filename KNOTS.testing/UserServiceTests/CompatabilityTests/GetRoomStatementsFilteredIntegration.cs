using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetRoomStatementsFilteredIntegration : UserServiceTestBase{
    [Fact]
    public void GetRoomStatemenets_Filtered() {
        var service = new CompatibilityService(Context, UserService);
        var res = service.GetRoomStatements("roomTopic", new List<string>{"Science"}, 10);
        Assert.NotEmpty(res);
        Assert.All(res, x => Assert.Equal("Science", x.Topic));
    }
}