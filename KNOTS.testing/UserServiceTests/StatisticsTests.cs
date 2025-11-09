using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KNOTS.Tests.Services;

public class UserServiceStatisticsTests : IDisposable {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    private readonly UserService _userService;

    public UserServiceStatisticsTests() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _logger = new LoggingService();
        _userService = new UserService(_context, _logger);
    }

    /// <summary>
    /// Tests that updating statistics for a user's first game sets all values correctly.
    /// Verifies TotalGamesPlayed is 1, BestMatchesCount is incremented, and average score is set.
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_FirstGame_UpdatesCorrectly() {
        // Arrange
        var username = "testuser";
        _userService.RegisterUser(username, "password123");
        var compatibilityScore = 85.5;

        // Act
        _userService.UpdateUserStatistics(username, compatibilityScore, wasBestMatch: true);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(1, user.TotalGamesPlayed);
        Assert.Equal(1, user.BestMatchesCount);
        Assert.Equal(85.5, user.AverageCompatibilityScore);
    }

    /// <summary>
    /// Tests that the average compatibility score is calculated correctly over multiple games.
    /// Verifies the running average formula: ((old_avg * (n-1)) + new_score) / n
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_MultipleGames_CalculatesAverageCorrectly() {
        // Arrange
        var username = "testuser";
        _userService.RegisterUser(username, "password123");

        // Act
        _userService.UpdateUserStatistics(username, 80.0, wasBestMatch: true);
        _userService.UpdateUserStatistics(username, 90.0, wasBestMatch: false);
        _userService.UpdateUserStatistics(username, 70.0, wasBestMatch: true);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(3, user.TotalGamesPlayed);
        Assert.Equal(2, user.BestMatchesCount);
        Assert.Equal(80.0, user.AverageCompatibilityScore, precision: 2);
    }

    /// <summary>
    /// Tests that BestMatchesCount is not incremented when wasBestMatch is false.
    /// Verifies only TotalGamesPlayed and AverageCompatibilityScore are updated.
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_NotBestMatch_DoesNotIncrementBestMatchCount() {
        // Arrange
        var username = "testuser";
        _userService.RegisterUser(username, "password123");

        // Act
        _userService.UpdateUserStatistics(username, 50.0, wasBestMatch: false);
        _userService.UpdateUserStatistics(username, 60.0, wasBestMatch: false);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(2, user.TotalGamesPlayed);
        Assert.Equal(0, user.BestMatchesCount);
        Assert.Equal(55.0, user.AverageCompatibilityScore);
    }

    /// <summary>
    /// Tests that username matching is case-insensitive when updating statistics.
    /// A user registered as "TestUser" can have stats updated using "TESTUSER".
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_CaseInsensitiveUsername_UpdatesCorrectly() {
        // Arrange
        var username = "TestUser";
        _userService.RegisterUser(username, "password123");

        // Act
        _userService.UpdateUserStatistics("TESTUSER", 75.0, wasBestMatch: true);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(1, user.TotalGamesPlayed);
        Assert.Equal(1, user.BestMatchesCount);
    }

    /// <summary>
    /// Tests that updating statistics for a non-existent user throws UserNotFoundException.
    /// Verifies proper error handling for invalid usernames.
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_NonExistentUser_ThrowsUserNotFoundException() {
        // Act & Assert
        Assert.Throws<UserNotFoundException>(() => 
            _userService.UpdateUserStatistics("nonexistent", 50.0, false));
    }

    /// <summary>
    /// Tests that a compatibility score of 0.0 is handled correctly.
    /// Verifies the system can process edge case scores without errors.
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_ZeroCompatibilityScore_UpdatesCorrectly() {
        // Arrange
        var username = "testuser";
        _userService.RegisterUser(username, "password123");

        // Act
        _userService.UpdateUserStatistics(username, 0.0, wasBestMatch: false);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(1, user.TotalGamesPlayed);
        Assert.Equal(0.0, user.AverageCompatibilityScore);
    }

    /// <summary>
    /// Tests that a perfect compatibility score of 100.0 is handled correctly.
    /// Verifies the system can process maximum score values without errors.
    /// </summary>
    [Fact]
    public void UpdateUserStatistics_HighCompatibilityScore_UpdatesCorrectly() {
        // Arrange
        var username = "testuser";
        _userService.RegisterUser(username, "password123");

        // Act
        _userService.UpdateUserStatistics(username, 100.0, wasBestMatch: true);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(100.0, user.AverageCompatibilityScore);
    }

    /// <summary>
    /// Tests that GetTotalUsersCount returns 0 when there are no users in the database.
    /// Verifies the method handles an empty database correctly.
    /// </summary>
    [Fact]
    public void GetTotalUsersCount_NoUsers_ReturnsZero() {
        // Act
        var count = _userService.GetTotalUsersCount();

        // Assert
        Assert.Equal(0, count);
    }

    /// <summary>
    /// Tests that GetTotalUsersCount returns the correct number of registered users.
    /// Verifies the method accurately counts all users in the database.
    /// </summary>
    [Fact]
    public void GetTotalUsersCount_MultipleUsers_ReturnsCorrectCount() {
        // Arrange
        _userService.RegisterUser("user1", "password123");
        _userService.RegisterUser("user2", "password456");
        _userService.RegisterUser("user3", "password789");

        // Act
        var count = _userService.GetTotalUsersCount();

        // Assert
        Assert.Equal(3, count);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}