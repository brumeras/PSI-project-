using KNOTS.Data;
using KNOTS.Models;
using Microsoft.EntityFrameworkCore;
using KNOTS.Exceptions;

namespace KNOTS.Services;

public class UserService {
    private readonly AppDbContext _context;
    private readonly LoggingService _logger;
    public string? CurrentUser { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);
    public event Action? OnAuthenticationChanged;
    
    public UserService(AppDbContext context, LoggingService logger) {
        _context = context;
        _logger = logger;
    }
    
   public (bool Success, string Message) RegisterUser(string username, string password) {
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) {return (false, "Username and password cannot be empty."); }
    if (username.Length < 3) {return (false, "Username must be at least 3 characters long."); }
    if (password.Length < 4) {return (false, "Password must be at least 4 characters long."); }
    
    try {
        var usernameLower = username.ToLower();
        if (_context.Users.Any(u => u.Username.ToLower() == usernameLower)) throw new UserAlreadyExistsException(username);
        
        var passwordHash = PasswordHasher.Hash(password);
        var newUser = new User {
            Username = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.Now,
            TotalGamesPlayed = 0,
            BestMatchesCount = 0,
            AverageCompatibilityScore = 0.0
        };
        _context.Users.Add(newUser);
        _context.SaveChanges();
        return (true, "Registration successful! You can now log in.");
    } 
    catch (UserAlreadyExistsException ex) {
        _logger.LogException(ex, $"User with this username already exists: {username}");
        return (false, "This username is already taken");
    }
    catch (DbUpdateException ex) {
        _logger.LogException(ex, $"Database error for user: {username}");
        return (false, "Unable to complete registration due to a database error");
    }
    catch (InvalidOperationException ex) {
        _logger.LogException(ex, $"Operation error for user: {username}");
        return (false, "Registration failed due to a system error");
    }
    catch (Exception ex) {
        _logger.LogException(ex, $"Unexpected error for user: {username}"); ;
        return (false, "An unexpected error occurred during registration");
    }
}
    public (bool Success, string Message) LoginUser(string username, string password) {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) {return (false, "Username and password cannot be empty.");}

        try{
            var user = _context.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            
            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash)) return (false, "Invalid username or password.");
                CurrentUser = user.Username;
                OnAuthenticationChanged?.Invoke();
                return (true, "Login successful");
        } catch (Exception ex){
                _logger.LogException(ex, $"Unexpected error during login: {username}");
                return (false, "An unexpected error occurred during login");
        }
    }
    public void LogoutUser() {
        CurrentUser = null;
        OnAuthenticationChanged?.Invoke();
    }
    public int GetTotalUsersCount() => _context.Users.Count();
    
    public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch) {
        var user = _context.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        if (user == null) return;

        user.TotalGamesPlayed++;
        if (wasBestMatch) user.BestMatchesCount++;

        user.AverageCompatibilityScore =
            ((user.AverageCompatibilityScore * (user.TotalGamesPlayed - 1)) + compatibilityScore)
            / user.TotalGamesPlayed;

        _context.SaveChanges();
    }
    public List<User> GetLeaderboard(int topCount = 10) {
        return _context.Users
            .AsEnumerable()
            .OrderBy(u => u)
            .Take(topCount)
            .ToList();
    }
    public int GetUserRank(string username) {
        var sortedUsers = _context.Users
            .AsEnumerable()
            .OrderByDescending(u => u)
            .ToList();
        var rank = sortedUsers.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
        return rank;
    }
}

