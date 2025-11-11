using KNOTS.Models;
using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class ClearRoomDataIntegration : UserServiceTestBase{
    [Fact]
    public void ClearRoomData()
    {
        Context.PlayerSwipes.AddRange(
            new PlayerSwipeRecord{RoomCode = "room1", PlayerUsername = "pirmas", StatementId = "s1", StatementText = "statement 1", AgreeWithStatement = true},
            new PlayerSwipeRecord{RoomCode = "room1", PlayerUsername = "antras", StatementId = "s2", StatementText = "statement 2", AgreeWithStatement = false},
            new PlayerSwipeRecord { RoomCode = "room2", PlayerUsername = "trecias", StatementId = "s3", StatementText = "statement 3", AgreeWithStatement = true }
        );
        Context.SaveChanges();
        var service = new CompatibilityService(Context, new UserService(Context, new LoggingService()));
        service.ClearRoomData("room1");
        
        Assert.Empty(Context.PlayerSwipes.Where(s => s.RoomCode == "room1"));
        Assert.Single(Context.PlayerSwipes.Where(s => s.RoomCode == "room2"));
    }
}