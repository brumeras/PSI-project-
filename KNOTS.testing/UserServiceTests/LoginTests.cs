using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KNOTS.Tests.Services;

public class UserServiceLoginTests : IDisposable {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    private readonly UserService _userService;

    public UserServiceLoginTests() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _logger = new LoggingService();
        _userService = new UserService(_context, _logger);
    }

    /// <summary>
    /// Tests that a user can successfully log in with correct credentials.
    /// Verifies CurrentUser is set and IsAuthenticated becomes true.
    /// </summary>
    [Fact]
    public void LoginUser_ValidCredentials_ReturnsSuccess() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        var result = _userService.LoginUser(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.Equal(username, _userService.CurrentUser);
        Assert.True(_userService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that login is case-insensitive for usernames.
    /// A user registered as "TestUser" can login with "TESTUSER".
    /// </summary>
    [Fact]
    public void LoginUser_CaseInsensitiveUsername_ReturnsSuccess() {
        // Arrange
        var username = "TestUser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        var result = _userService.LoginUser("TESTUSER", password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.True(_userService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that login fails when an incorrect username is provided.
    /// Verifies CurrentUser remains null and IsAuthenticated stays false.
    /// </summary>
    [Fact]
    public void LoginUser_InvalidUsername_ReturnsFailure() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        var result = _userService.LoginUser("wronguser", password);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid username or password", result.Message);
        Assert.Null(_userService.CurrentUser);
        Assert.False(_userService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that login fails when an incorrect password is provided.
    /// Verifies CurrentUser remains null and IsAuthenticated stays false.
    /// </summary>
    [Fact]
    public void LoginUser_InvalidPassword_ReturnsFailure() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        var result = _userService.LoginUser(username, "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid username or password", result.Message);
        Assert.Null(_userService.CurrentUser);
        Assert.False(_userService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that login throws ArgumentException when credentials are empty or whitespace.
    /// Validates all combinations of empty username/password inputs.
    /// </summary>
    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    [InlineData("", "")]
    [InlineData("   ", "password")]
    [InlineData("username", "   ")]
    public void LoginUser_EmptyCredentials_ThrowsArgumentException(string username, string password) {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _userService.LoginUser(username, password));
    }

    /// <summary>
    /// Tests that the OnAuthenticationChanged event is triggered after successful login.
    /// Ensures subscribers are notified when authentication state changes.
    /// </summary>
    [Fact]
    public void LoginUser_TriggersAuthenticationChangedEvent() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        var eventTriggered = false;
        _userService.OnAuthenticationChanged += () => eventTriggered = true;

        // Act
        _userService.LoginUser(username, password);

        // Assert
        Assert.True(eventTriggered);
    }

    /// <summary>
    /// Tests that logging in with a different user updates the CurrentUser property.
    /// Verifies that the system correctly switches between logged-in users.
    /// </summary>
    [Fact]
    public void LoginUser_MultipleLogins_UpdatesCurrentUser() {
        // Arrange
        _userService.RegisterUser("user1", "password123");
        _userService.RegisterUser("user2", "password456");

        // Act
        _userService.LoginUser("user1", "password123");
        Assert.Equal("user1", _userService.CurrentUser);

        _userService.LoginUser("user2", "password456");

        // Assert
        Assert.Equal("user2", _userService.CurrentUser);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}