using KNOTS.Data;
using KNOTS.Services;
using Microsoft.EntityFrameworkCore;


namespace KNOTS.Tests.Services;

public class UserServiceLogoutTests : IDisposable {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    private readonly UserService _userService;

    public UserServiceLogoutTests() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _logger = new LoggingService();
        _userService = new UserService(_context, _logger);
    }

    /// <summary>
    /// Tests that logging out clears the CurrentUser and sets IsAuthenticated to false.
    /// Verifies the user is properly logged out after being logged in.
    /// </summary>
    [Fact]
    public void LogoutUser_WhenLoggedIn_ClearsCurrentUser() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        _userService.LoginUser(username, password);
        Assert.True(_userService.IsAuthenticated);

        // Act
        _userService.LogoutUser();

        // Assert
        Assert.Null(_userService.CurrentUser);
        Assert.False(_userService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that the OnAuthenticationChanged event is triggered when a user logs out.
    /// Ensures subscribers are notified of the authentication state change.
    /// </summary>
    [Fact]
    public void LogoutUser_TriggersAuthenticationChangedEvent() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        _userService.LoginUser(username, password);
        
        var eventTriggered = false;
        _userService.OnAuthenticationChanged += () => eventTriggered = true;

        // Act
        _userService.LogoutUser();

        // Assert
        Assert.True(eventTriggered);
    }

    /// <summary>
    /// Tests that calling LogoutUser when no user is logged in does not throw an exception.
    /// Verifies the method handles the edge case gracefully.
    /// </summary>
    [Fact]
    public void LogoutUser_WhenNotLoggedIn_DoesNotThrowException() {
        // Act & Assert
        var exception = Record.Exception(() => _userService.LogoutUser());
        Assert.Null(exception);
        Assert.False(_userService.IsAuthenticated);
    }

    /// <summary>
    /// Tests that calling LogoutUser multiple times in succession does not throw an exception.
    /// Verifies the method is idempotent and handles repeated calls safely.
    /// </summary>
    [Fact]
    public void LogoutUser_MultipleLogouts_DoesNotThrowException() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        _userService.LoginUser(username, password);

        // Act & Assert
        _userService.LogoutUser();
        var exception = Record.Exception(() => _userService.LogoutUser());
        Assert.Null(exception);
        Assert.False(_userService.IsAuthenticated);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}