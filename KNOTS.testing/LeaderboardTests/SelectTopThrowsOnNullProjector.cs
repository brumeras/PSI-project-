using KNOTS.Services;

namespace TestProject1.LeaderboardTests;

public class SelectTopThrowsOnNullProjector {
    [Fact]
    public void SelectTop_ThrowsOnNullProjector() {
        var users = new List<User> { new User { Username = "pirmas" } };
        var leaderboard = new Leaderboard<User>(users);
        Assert.Throws<ArgumentNullException>(() => leaderboard.SelectTop<string>(1, null!));
    }
}