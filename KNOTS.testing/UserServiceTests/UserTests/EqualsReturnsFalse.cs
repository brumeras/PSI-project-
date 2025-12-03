using KNOTS.Services;

namespace TestProject1.UserServiceTests.UserTests;

public class EqualsReturnsFalse {
    [Fact]
    public void EqualsReturnsFalseForValues() {
        var user1 = new User {
            Username = "Pirmas",
            AverageCompatibilityScore = 90.5,
            BestMatchesCount = 5,
            TotalGamesPlayed = 10
        };
        var user2 = new User {
            Username = "Antras",
            AverageCompatibilityScore = 90.5,
            BestMatchesCount = 5,
            TotalGamesPlayed = 10
        };
        Assert.False(user1.Equals(user2));
    }
}