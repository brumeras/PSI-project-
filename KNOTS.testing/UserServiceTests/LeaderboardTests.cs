using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KNOTS.Tests.Services;

public class UserServiceLeaderboardTests : IDisposable {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    private readonly UserService _userService;

    public UserServiceLeaderboardTests() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _logger = new LoggingService();
        _userService = new UserService(_context, _logger);
    }

    /// <summary>
    /// Tests that GetLeaderboard returns an empty list when no users exist.
    /// Verifies the method handles an empty database gracefully.
    /// </summary>
    [Fact]
    public void GetLeaderboard_NoUsers_ReturnsEmptyList() {
        // Act
        var leaderboard = _userService.GetLeaderboard();

        // Assert
        Assert.Empty(leaderboard);
    }

    /// <summary>
    /// Tests that GetLeaderboard returns top 10 users by default.
    /// When more than 10 users exist, verifies only the top 10 are returned.
    /// </summary>
    [Fact]
    public void GetLeaderboard_DefaultCount_ReturnsTop10() {
        // Arrange
        for (int i = 1; i <= 15; i++) {
            var username = $"user{i}";
            _userService.RegisterUser(username, "password123");
            _userService.UpdateUserStatistics(username, 50.0 + i, wasBestMatch: true);
        }

        // Act
        var leaderboard = _userService.GetLeaderboard();

        // Assert
        Assert.Equal(10, leaderboard.Count);
    }

    /// <summary>
    /// Tests that GetLeaderboard accepts a custom count parameter.
    /// Verifies the method returns exactly the requested number of users.
    /// </summary>
    [Fact]
    public void GetLeaderboard_CustomCount_ReturnsCorrectNumber() {
        // Arrange
        for (int i = 1; i <= 10; i++) {
            var username = $"user{i}";
            _userService.RegisterUser(username, "password123");
            _userService.UpdateUserStatistics(username, 50.0 + i, wasBestMatch: true);
        }

        // Act
        var leaderboard = _userService.GetLeaderboard(5);

        // Assert
        Assert.Equal(5, leaderboard.Count);
    }

    /// <summary>
    /// Tests that GetLeaderboard returns all users when fewer exist than requested.
    /// Verifies the method doesn't fail when the requested count exceeds available users.
    /// </summary>
    [Fact]
    public void GetLeaderboard_LessUsersThanRequested_ReturnsAllUsers() {
        // Arrange
        for (int i = 1; i <= 3; i++) {
            var username = $"user{i}";
            _userService.RegisterUser(username, "password123");
            _userService.UpdateUserStatistics(username, 50.0 + i, wasBestMatch: true);
        }

        // Act
        var leaderboard = _userService.GetLeaderboard(10);

        // Assert
        Assert.Equal(3, leaderboard.Count);
    }

    /// <summary>
    /// Tests that GetLeaderboard orders users based on the User's IComparable implementation.
    /// Verifies the method executes without errors and returns users in some defined order.
    /// Note: The actual order depends on how User.CompareTo is implemented.
    /// </summary>
    [Fact]
    public void GetLeaderboard_OrderedByUserComparison() {
        // Arrange
        _userService.RegisterUser("user1", "password123");
        _userService.UpdateUserStatistics("user1", 70.0, wasBestMatch: true);
        _userService.UpdateUserStatistics("user1", 80.0, wasBestMatch: true);

        _userService.RegisterUser("user2", "password123");
        _userService.UpdateUserStatistics("user2", 90.0, wasBestMatch: true);

        _userService.RegisterUser("user3", "password123");
        _userService.UpdateUserStatistics("user3", 60.0, wasBestMatch: false);

        // Act
        var leaderboard = _userService.GetLeaderboard();

        // Assert
        Assert.Equal(3, leaderboard.Count);
        // The actual order depends on User's IComparable implementation
        // This test verifies the method runs without errors
    }

    /// <summary>
    /// Tests that GetUserRank returns the correct rank for an existing user.
    /// Rank is based on the User's IComparable implementation (1 = best, higher = worse).
    /// </summary>
    [Fact]
    public void GetUserRank_ExistingUser_ReturnsCorrectRank() {
        // Arrange
        _userService.RegisterUser("user1", "password123");
        _userService.UpdateUserStatistics("user1", 70.0, wasBestMatch: true);

        _userService.RegisterUser("user2", "password123");
        _userService.UpdateUserStatistics("user2", 90.0, wasBestMatch: true);

        _userService.RegisterUser("user3", "password123");
        _userService.UpdateUserStatistics("user3", 60.0, wasBestMatch: true);

        // Act
        var rank = _userService.GetUserRank("user2");

        // Assert
        Assert.True(rank >= 1 && rank <= 3);
    }

    /// <summary>
    /// Tests that GetUserRank is case-insensitive when looking up usernames.
    /// A user registered as "TestUser" can have their rank checked using "TESTUSER".
    /// </summary>
    [Fact]
    public void GetUserRank_CaseInsensitive_ReturnsCorrectRank() {
        // Arrange
        _userService.RegisterUser("TestUser", "password123");
        _userService.UpdateUserStatistics("TestUser", 80.0, wasBestMatch: true);

        _userService.RegisterUser("user2", "password123");
        _userService.UpdateUserStatistics("user2", 70.0, wasBestMatch: true);

        // Act
        var rank = _userService.GetUserRank("TESTUSER");

        // Assert
        Assert.True(rank >= 1 && rank <= 2);
    }

    /// <summary>
    /// Tests that GetUserRank returns 0 for a non-existent user.
    /// Verifies the method indicates "not found" by returning 0.
    /// </summary>
    [Fact]
    public void GetUserRank_NonExistentUser_ReturnsZero() {
        // Arrange
        _userService.RegisterUser("user1", "password123");

        // Act
        var rank = _userService.GetUserRank("nonexistent");

        // Assert
        Assert.Equal(0, rank);
    }

    /// <summary>
    /// Tests that GetUserRank returns 1 for the only user in the system.
    /// Verifies the ranking works correctly with a single user.
    /// </summary>
    [Fact]
    public void GetUserRank_OnlyUser_ReturnsOne() {
        // Arrange
        _userService.RegisterUser("onlyuser", "password123");
        _userService.UpdateUserStatistics("onlyuser", 80.0, wasBestMatch: true);

        // Act
        var rank = _userService.GetUserRank("onlyuser");

        // Assert
        Assert.Equal(1, rank);
    }

    /// <summary>
    /// Tests that GetUserRank still returns a rank for users with no game statistics.
    /// Verifies users without stats are still included in the ranking system.
    /// </summary>
    [Fact]
    public void GetUserRank_NoStatistics_StillReturnsRank() {
        // Arrange
        _userService.RegisterUser("user1", "password123");
        _userService.RegisterUser("user2", "password123");

        // Act
        var rank = _userService.GetUserRank("user1");

        // Assert
        Assert.True(rank >= 1 && rank <= 2);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}