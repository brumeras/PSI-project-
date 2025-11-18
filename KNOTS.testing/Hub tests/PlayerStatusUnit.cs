using KNOTS.Hubs;

namespace TestProject1.Hub_tests;

public class PlayerStatusUnit {
    [Fact]
    public void PlayerStatusGetWorks()
    {
        var now = DateTime.UtcNow;
        var status = new PlayerStatus
        {
            Username = "Player1",
            IsOnline = true,
            Timestamp = now
        };

        Assert.Equal("Player1", status.Username);
        Assert.True(status.IsOnline);
        Assert.Equal(now, status.Timestamp);
    }
    
}