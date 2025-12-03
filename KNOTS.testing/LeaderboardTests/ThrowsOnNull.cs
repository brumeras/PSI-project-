using KNOTS.Services;

namespace TestProject1.LeaderboardTests;

public class ThrowsOnNull {
    [Fact]
    public void ConstructorThrowsOnNullPlayers() {
        Assert.Throws<ArgumentNullException>(() => new Leaderboard<User>(null!));
    }
}