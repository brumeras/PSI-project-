using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetRoomStatementsCountIntegration : UserServiceTestBase{
    [Fact]
    public void GetRoomStatements_Count() {
        var service = new CompatibilityService(Context, UserService);
        var many = service.GetRoomStatements("roomCount", null, 5);
        Assert.True(many.Count <= 5);
        //cia atvejis, kai nera kazkokio topic, tai grazinam tiesiog kas egzistuoja
        var sml = service.GetRoomStatements("room", new List<string>{"someTopic"}, 10);
        Assert.True(sml.Count <= 10);
        }
}