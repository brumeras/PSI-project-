using KNOTS.Data;
using KNOTS.Services;
using Microsoft.EntityFrameworkCore;

namespace KNOTS.Tests.Services;

public class UserServiceRegisterTests : IDisposable {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    private readonly UserService _userService;

    public UserServiceRegisterTests() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _logger = new LoggingService();
        _userService = new UserService(_context, _logger);
    }

    /// <summary>
    /// Tests that a user can successfully register with valid credentials.
    /// Verifies the user is added to the database with correct initial values.
    /// </summary>
    [Fact]
    public void RegisterUser_ValidCredentials_ReturnsSuccess() {
        // Arrange
        var username = "testuser";
        var password = "password123";

        // Act
        var result = _userService.RegisterUser(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful! You can now log in.", result.Message);
        
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
        Assert.Equal(0, user.TotalGamesPlayed);
        Assert.Equal(0, user.BestMatchesCount);
        Assert.Equal(0.0, user.AverageCompatibilityScore);
    }

    /// <summary>
    /// Tests that registration fails when username or password is empty or whitespace.
    /// Should throw ArgumentException for any combination of empty/whitespace credentials.
    /// </summary>
    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    [InlineData("", "")]
    [InlineData("   ", "password")]
    [InlineData("username", "   ")]
    public void RegisterUser_EmptyCredentials_ThrowsArgumentException(string username, string password) {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _userService.RegisterUser(username, password));
    }

    /// <summary>
    /// Tests that registration fails when username is shorter than 3 characters.
    /// Verifies the error message indicates the minimum length requirement.
    /// </summary>
    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    [InlineData("")]
    public void RegisterUser_UsernameTooShort_ThrowsArgumentException(string username) {
        // Arrange
        var password = "password123";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _userService.RegisterUser(username, password));
        Assert.Contains("Username must be at least 3 characters long", exception.Message);
    }

    /// <summary>
    /// Tests that registration fails when password is shorter than 4 characters.
    /// Verifies the error message indicates the minimum length requirement.
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("ab")]
    [InlineData("a")]
    public void RegisterUser_PasswordTooShort_ThrowsArgumentException(string password) {
        // Arrange
        var username = "testuser";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _userService.RegisterUser(username, password));
        Assert.Contains("Password must be at least 4 characters long", exception.Message);
    }

    /// <summary>
    /// Tests that registration fails when attempting to register with an existing username.
    /// Ensures duplicate usernames are not allowed in the system.
    /// </summary>
    [Fact]
    public void RegisterUser_DuplicateUsername_ReturnsFailure() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        var result = _userService.RegisterUser(username, "differentpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that username matching is case-insensitive during registration.
    /// A user cannot register with "TestUser" if "TESTUSER" already exists.
    /// </summary>
    [Fact]
    public void RegisterUser_DuplicateUsernameDifferentCase_ReturnsFailure() {
        // Arrange
        var username = "TestUser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        var result = _userService.RegisterUser("TESTUSER", "differentpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that passwords are hashed before being stored in the database.
    /// Verifies the stored password hash is not equal to the plain text password.
    /// </summary>
    [Fact]
    public void RegisterUser_PasswordIsHashed_NotStoredInPlainText() {
        // Arrange
        var username = "testuser";
        var password = "password123";

        // Act
        _userService.RegisterUser(username, password);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);
    }

    /// <summary>
    /// Tests that the CreatedAt timestamp is set correctly during registration.
    /// Verifies the timestamp falls within a reasonable time range around the registration.
    /// </summary>
    [Fact]
    public void RegisterUser_SetsCreatedAtDate() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var beforeRegistration = DateTime.Now.AddSeconds(-1);

        // Act
        _userService.RegisterUser(username, password);
        var afterRegistration = DateTime.Now.AddSeconds(1);

        // Assert
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
        Assert.InRange(user.CreatedAt, beforeRegistration, afterRegistration);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}