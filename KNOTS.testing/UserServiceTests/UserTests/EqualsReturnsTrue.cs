using KNOTS.Services;

namespace TestProject1.UserServiceTests.UserTests;

public class EqualsReturnsTrue {
    [Fact]
    public void EqualsReturnsTrueForValues() {
        var user1 = new User {
            Username = "Pirmas",
            AverageCompatibilityScore = 90.5,
            BestMatchesCount = 5,
            TotalGamesPlayed = 10
        };
        var user2 = new User {
            Username = "pirmas",
            AverageCompatibilityScore = 90.5,
            BestMatchesCount = 5,
            TotalGamesPlayed = 10
        };
        Assert.True(user1.Equals(user2));
        Assert.True(user2.Equals(user1));
        Assert.Equal(user1.GetHashCode(), user2.GetHashCode());
    }
}