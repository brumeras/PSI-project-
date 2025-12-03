using KNOTS.Services;

namespace TestProject1.UserServiceTests.UserTests;

public class GetHashCode {
    [Fact]
    public void GetHashCodeDifferentHashCodes() {
        var user1 = new User {
            Username = "pirmas",
            AverageCompatibilityScore = 90.5,
            BestMatchesCount = 5,
            TotalGamesPlayed = 10
        };
        var user2 = new User {
            Username = "antras",
            AverageCompatibilityScore = 80.0,
            BestMatchesCount = 2,
            TotalGamesPlayed = 5
        };
        Assert.NotEqual(user1.GetHashCode(), user2.GetHashCode());
    }

}