using KNOTS.Services;

namespace TestProject1.LeaderboardTests;

public class SelectTopThrowsOnNegativeCount {
    [Fact]
    public void SelectTop_ThrowsOnNegativeCount(){
        var users = new List<User> { new User { Username = "pirmas" } };
        var leaderboard = new Leaderboard<User>(users);
        Assert.Throws<ArgumentOutOfRangeException>(() => leaderboard.SelectTop(-1, u => u.Username));
    }
}