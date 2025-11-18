using System.Collections.Generic;
using System.Linq;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class RoomCreationFetchStatementsPlayGameUpdatesLeaderboard : EndToEndTestBase
{
    [Fact]
    public void RoomCreationThroughGameFlow_UpdatesLeaderboardAndRanks()
    {
        RegisterTestUsers("host", "guest-one", "guest-two");

        var roomRepository = new RoomRepository();
        var roomManager = new RoomManager(roomRepository, new RoomCodeGenerator());
        var room = roomManager.CreateRoom("conn-host", "host");
        room.AddPlayer(new GamePlayer("conn-g1", "guest-one"));
        room.AddPlayer(new GamePlayer("conn-g2", "guest-two"));
        
        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "LB-1", Text = "Statement 1", Topic = "General" },
            new GameStatement { Id = "LB-2", Text = "Statement 2", Topic = "General" }
        });
        
        CompatibilityService.SaveSwipe(room.RoomCode, "guest-two", "LB-1", true);
        CompatibilityService.SaveSwipe(room.RoomCode, "guest-two", "LB-2", false);

        CompatibilityService.SaveSwipe(room.RoomCode, "host", "LB-1", true);
        CompatibilityService.SaveSwipe(room.RoomCode, "host", "LB-2", false);
        
        CompatibilityService.SaveSwipe(room.RoomCode, "guest-one", "LB-1", true);
        CompatibilityService.SaveSwipe(room.RoomCode, "guest-one", "LB-2", false);

        var players = room.Players.Select(p => p.Username).ToList();
        CompatibilityService.SaveGameToHistory(room.RoomCode, players);

        var leaderboard = UserService.GetLeaderboard(3);
        
        Assert.Equal(new[] { "guest-one", "host", "guest-two" }, leaderboard.Select(u => u.Username).ToArray());
        Assert.Equal(new[] { 100.0, 100.0, 100.0 }, leaderboard.Select(u => u.AverageCompatibilityScore).ToArray());

        Assert.Equal(2, UserService.GetUserRank("host"));
        Assert.Equal(1, UserService.GetUserRank("guest-two"));
        Assert.Equal(3, UserService.GetUserRank("guest-one"));
    }
}
