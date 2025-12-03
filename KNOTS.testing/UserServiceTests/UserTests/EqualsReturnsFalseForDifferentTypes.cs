using KNOTS.Services;

namespace TestProject1.UserServiceTests.UserTests;

public class EqualsReturnsFalseForDifferentTypes {
    [Fact]
    public void EqualsReturnsFalseForDifTypes() {
        var user = new User {
            Username = "pirmas",
            AverageCompatibilityScore = 90.5,
            BestMatchesCount = 5,
            TotalGamesPlayed = 10
        };
        Assert.False(user.Equals(null));
        Assert.False(user.Equals("some string")); // different type
    }

}