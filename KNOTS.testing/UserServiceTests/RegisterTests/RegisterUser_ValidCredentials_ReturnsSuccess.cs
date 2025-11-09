namespace KNOTS.testing.UserServiceTests.RegisterTests;

/// <summary>
/// Tests that a user can successfully register with valid credentials.
/// Verifies the user is added to the database with correct initial values.
/// </summary>
public class RegisterUser_ValidCredentials_ReturnsSuccess : UserServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";

        // Act
        var result = UserService.RegisterUser(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful! You can now log in.", result.Message);
        
        var user = Context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
        Assert.Equal(0, user.TotalGamesPlayed);
        Assert.Equal(0, user.BestMatchesCount);
        Assert.Equal(0.0, user.AverageCompatibilityScore);
    }
}