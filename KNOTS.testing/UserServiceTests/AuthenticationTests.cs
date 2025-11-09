using KNOTS.Data;
using KNOTS.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KNOTS.Tests.Services;

public class UserServiceAuthenticationTests : IDisposable {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    private readonly UserService _userService;

    public UserServiceAuthenticationTests() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _logger = new LoggingService();
        _userService = new UserService(_context, _logger);
    }

    /// <summary>
    /// Tests that IsAuthenticated is false and CurrentUser is null when UserService is created.
    /// Verifies the initial authentication state before any login attempts.
    /// </summary>
    [Fact]
    public void IsAuthenticated_InitialState_ReturnsFalse() {
        // Assert
        Assert.False(_userService.IsAuthenticated);
        Assert.Null(_userService.CurrentUser);
    }

    /// <summary>
    /// Tests that IsAuthenticated becomes true and CurrentUser is set after successful login.
    /// Verifies the authentication state changes correctly after login.
    /// </summary>
    [Fact]
    public void IsAuthenticated_AfterLogin_ReturnsTrue() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        _userService.LoginUser(username, password);

        // Assert
        Assert.True(_userService.IsAuthenticated);
        Assert.NotNull(_userService.CurrentUser);
    }

    /// <summary>
    /// Tests that IsAuthenticated becomes false and CurrentUser is cleared after logout.
    /// Verifies the authentication state is properly reset when logging out.
    /// </summary>
    [Fact]
    public void IsAuthenticated_AfterLogout_ReturnsFalse() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        _userService.LoginUser(username, password);

        // Act
        _userService.LogoutUser();

        // Assert
        Assert.False(_userService.IsAuthenticated);
        Assert.Null(_userService.CurrentUser);
    }

    /// <summary>
    /// Tests that CurrentUser property returns the username of the logged-in user.
    /// Verifies the correct username is stored after successful authentication.
    /// </summary>
    [Fact]
    public void CurrentUser_AfterSuccessfulLogin_ReturnsUsername() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        _userService.LoginUser(username, password);

        // Assert
        Assert.Equal(username, _userService.CurrentUser);
    }

    /// <summary>
    /// Tests that CurrentUser remains null when login fails due to incorrect credentials.
    /// Verifies the property is not modified on failed login attempts.
    /// </summary>
    [Fact]
    public void CurrentUser_AfterFailedLogin_RemainsNull() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act
        _userService.LoginUser(username, "wrongpassword");

        // Assert
        Assert.Null(_userService.CurrentUser);
    }

    /// <summary>
    /// Tests that OnAuthenticationChanged event fires when a user logs in successfully.
    /// Verifies event subscribers are notified of authentication state changes.
    /// </summary>
    [Fact]
    public void OnAuthenticationChanged_SubscribedEvent_TriggersOnLogin() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        
        var eventCount = 0;
        _userService.OnAuthenticationChanged += () => eventCount++;

        // Act
        _userService.LoginUser(username, password);

        // Assert
        Assert.Equal(1, eventCount);
    }

    /// <summary>
    /// Tests that OnAuthenticationChanged event fires when a user logs out.
    /// Verifies event subscribers are notified when authentication state changes to logged out.
    /// </summary>
    [Fact]
    public void OnAuthenticationChanged_SubscribedEvent_TriggersOnLogout() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        _userService.LoginUser(username, password);
        
        var eventCount = 0;
        _userService.OnAuthenticationChanged += () => eventCount++;

        // Act
        _userService.LogoutUser();

        // Assert
        Assert.Equal(1, eventCount);
    }

    /// <summary>
    /// Tests that all subscribers to OnAuthenticationChanged are notified when the event fires.
    /// Verifies the event system supports multiple subscribers correctly.
    /// </summary>
    [Fact]
    public void OnAuthenticationChanged_MultipleSubscribers_AllGetNotified() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        
        var subscriber1Called = false;
        var subscriber2Called = false;
        _userService.OnAuthenticationChanged += () => subscriber1Called = true;
        _userService.OnAuthenticationChanged += () => subscriber2Called = true;

        // Act
        _userService.LoginUser(username, password);

        // Assert
        Assert.True(subscriber1Called);
        Assert.True(subscriber2Called);
    }

    /// <summary>
    /// Tests that login succeeds even when no subscribers are attached to OnAuthenticationChanged.
    /// Verifies the event is nullable and doesn't cause errors when no handlers are attached.
    /// </summary>
    [Fact]
    public void OnAuthenticationChanged_NoSubscribers_DoesNotThrowException() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);

        // Act & Assert
        var exception = Record.Exception(() => _userService.LoginUser(username, password));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that OnAuthenticationChanged event does NOT fire when login fails.
    /// Verifies subscribers are only notified on successful authentication state changes.
    /// </summary>
    [Fact]
    public void OnAuthenticationChanged_FailedLogin_DoesNotTriggerEvent() {
        // Arrange
        var username = "testuser";
        var password = "password123";
        _userService.RegisterUser(username, password);
        
        var eventTriggered = false;
        _userService.OnAuthenticationChanged += () => eventTriggered = true;

        // Act
        _userService.LoginUser(username, "wrongpassword");

        // Assert
        Assert.False(eventTriggered);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}