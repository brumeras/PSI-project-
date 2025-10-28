using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using KNOTS.Data;
using KNOTS.Models;
using Microsoft.EntityFrameworkCore;

namespace KNOTS.Services;

/// <summary>
/// Provides user management services such as registration, authentication, 
/// statistics tracking, and leaderboard generation.
/// </summary>
public class UserService {
    private readonly AppDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="context">The application database context used to access user data.</param>
    public UserService(AppDbContext context) {
        _context = context;
        Console.WriteLine("🔧 UserService created");
    }
    
    /// <summary>
    /// Occurs when the authentication state changes (e.g., user logs in or out).
    /// </summary>
    public event Action? OnAuthenticationChanged;
    
    /// <summary>
    /// Gets the username of the currently authenticated user, or <c>null</c> if no user is logged in.
    /// </summary>
    public string? CurrentUser { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);
    
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="username">The desired username of the new user.</param>
    /// <param name="password">The password for the new account.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><term>Success</term><description><c>true</c> if registration succeeded; otherwise <c>false</c>.</description></item>
    /// <item><term>Message</term><description>A human-readable message describing the result.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method performs validation, hashes the password using BCrypt, and saves the user in the database.
    /// </remarks>
    public (bool Success, string Message) RegisterUser(string username, string password) {
        Console.WriteLine("\n=== REGISTRATION ATTEMPT ===");
        Console.WriteLine($"Username: '{username}'");
        Console.WriteLine($"Password length: {password?.Length ?? 0}");
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) {
            Console.WriteLine("❌ Empty username or password");
            return (false, "Username and password cannot be empty.");
        }
        if (username.Length < 3) {
            Console.WriteLine("❌ Username too short");
            return (false, "Username must be at least 3 characters long.");
        }
        if (password.Length < 4) {
            Console.WriteLine("❌ Password too short");
            return (false, "Password must be at least 4 characters long.");
        }
        try {
            Console.WriteLine("Checking if username exists...");
            // PATAISYTA: naudojame ToLower() vietoj StringComparison
            var usernameLower = username.ToLower();
            var existingUser = _context.Users
                .Where(u => u.Username.ToLower() == usernameLower)
                .FirstOrDefault();
            if (existingUser != null) {
                Console.WriteLine($"❌ Username '{username}' already exists");
                return (false, "This username is already taken.");
            }

            Console.WriteLine("Creating password hash...");
            var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
            Console.WriteLine($"Hash created, starts with: {passwordHash.Substring(0, Math.Min(20, passwordHash.Length))}...");

            var newUser = new User {
                Username = username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now,
                TotalGamesPlayed = 0,
                BestMatchesCount = 0,
                AverageCompatibilityScore = 0.0
            };

            Console.WriteLine("Adding user to database...");
            _context.Users.Add(newUser);

            Console.WriteLine("Saving changes to database...");
            var saveResult = _context.SaveChanges();
            Console.WriteLine($"SaveChanges returned: {saveResult}");

            var totalUsers = _context.Users.Count();
            Console.WriteLine($"✅ User '{username}' registered successfully");
            Console.WriteLine($"📊 Total users in DB now: {totalUsers}");

            return (true, "Registration successful! You can now log in.");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ ERROR during registration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return (false, $"Registration error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Attempts to authenticate a user with the specified credentials.
    /// </summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><term>Success</term><description><c>true</c> if login was successful; otherwise <c>false</c>.</description></item>
    /// <item><term>Message</term><description>A description of the login result.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// If login succeeds, the <see cref="CurrentUser"/> property is set and the <see cref="OnAuthenticationChanged"/> event is triggered.
    /// </remarks>
    public (bool Success, string Message) LoginUser(string username, string password) {
        Console.WriteLine("\n=== LOGIN ATTEMPT ===");
        Console.WriteLine($"Username: '{username}'");
        Console.WriteLine($"Password length: {password?.Length ?? 0}");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) {
            Console.WriteLine("❌ Empty username or password");
            return (false, "Username and password cannot be empty.");
        }

        try {
            var totalUsers = _context.Users.Count();
            Console.WriteLine($"📊 Total users in DB: {totalUsers}");

            if (totalUsers == 0) {
                Console.WriteLine("⚠️ Database is empty! Please register first.");
                return (false, "No users found. Please register first.");
            }

            // List all users for debugging
            Console.WriteLine("Users in database:");
            foreach (var u in _context.Users.ToList()) {
                Console.WriteLine($"  - {u.Username} (created: {u.CreatedAt})");
            }
            Console.WriteLine($"Searching for user: '{username}'");
            // PATAISYTA: naudojame ToLower() vietoj StringComparison
            var usernameLower = username.ToLower();
            var user = _context.Users
                .Where(u => u.Username.ToLower() == usernameLower)
                .FirstOrDefault();

            Console.WriteLine($"User found in DB: {user != null}");

            if (user != null) {
                Console.WriteLine($"Found user: {user.Username}");
                Console.WriteLine(
                    $"Stored hash starts with: {user.PasswordHash.Substring(0, Math.Min(20, user.PasswordHash.Length))}...");
                Console.WriteLine("Verifying password...");

                bool passwordValid = BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash);
                Console.WriteLine($"Password verification result: {passwordValid}");

                if (passwordValid) {
                    CurrentUser = user.Username;
                    Console.WriteLine($"✅ Login successful! CurrentUser set to: '{CurrentUser}'");
                    Console.WriteLine($"IsAuthenticated: {IsAuthenticated}");

                    OnAuthenticationChanged?.Invoke();
                    Console.WriteLine("OnAuthenticationChanged event invoked");

                    return (true, "Login successful!");
                }else {
                    Console.WriteLine("❌ Password verification failed - incorrect password");
                    return (false, "Invalid username or password.");
                }
            }else {
                Console.WriteLine($"❌ User '{username}' not found in database");
                return (false, "Invalid username or password.");
            }
        }catch (Exception ex) {
                Console.WriteLine($"❌ ERROR during login: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return (false, $"Login error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Logs out the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Clears <see cref="CurrentUser"/> and triggers the <see cref="OnAuthenticationChanged"/> event.
    /// </remarks>
    public void LogoutUser() {
        Console.WriteLine($"\n=== LOGOUT ===");
        Console.WriteLine($"Logging out user: {CurrentUser}");
        CurrentUser = null;
        OnAuthenticationChanged?.Invoke();
        Console.WriteLine("✅ Logout successful");
    }
    
    /// <summary>
    /// Retrieves the total number of registered users in the system.
    /// </summary>
    /// <returns>The number of users stored in the database.</returns>
    public int GetTotalUsersCount() {
        var count = _context.Users.Count();
        Console.WriteLine($"GetTotalUsersCount: {count}");
        return count;
    }
    
    /// <summary>
    /// Updates the statistical data of a specified user after a game.
    /// </summary>
    /// <param name="username">The username of the user to update.</param>
    /// <param name="compatibilityScore">The compatibility score obtained in the game.</param>
    /// <param name="wasBestMatch">Indicates whether the user was part of the best-matching pair.</param>
    /// <remarks>
    /// This method updates the user's total games played, recalculates their average compatibility score, 
    /// and increments their best match count if applicable.
    /// </remarks>
    public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch) {
        Console.WriteLine($"\n=== UPDATE USER STATISTICS ===");
        Console.WriteLine($"Username: {username}, Score: {compatibilityScore}, BestMatch: {wasBestMatch}");

        // PATAISYTA: naudojame ToLower()
        var usernameLower = username.ToLower();
        var user = _context.Users
            .Where(u => u.Username.ToLower() == usernameLower)
            .FirstOrDefault();

        if (user != null) {
            user.TotalGamesPlayed++;

            if (wasBestMatch) { user.BestMatchesCount++; }

            user.AverageCompatibilityScore =
                ((user.AverageCompatibilityScore * (user.TotalGamesPlayed - 1)) + compatibilityScore)
                / user.TotalGamesPlayed;

            _context.SaveChanges();

            Console.WriteLine($"✅ Updated stats for {username}:");
            Console.WriteLine($"   Games: {user.TotalGamesPlayed}");
            Console.WriteLine($"   Avg Score: {user.AverageCompatibilityScore:F2}");
            Console.WriteLine($"   Best Matches: {user.BestMatchesCount}");
        }else { Console.WriteLine($"❌ User '{username}' not found for statistics update"); }
    }
    
    /// <summary>
    /// Retrieves a sorted list of top users for the leaderboard.
    /// </summary>
    /// <param name="topCount">The number of top users to include in the leaderboard. Defaults to 10.</param>
    /// <returns>A list of users ordered by their ranking score.</returns>
    /// <remarks>
    /// The sorting logic relies on the <see cref="User"/> model's <c>IComparable</c> implementation.
    /// </remarks>
    public List<User> GetLeaderboard(int topCount = 10) {
        Console.WriteLine($"\n=== GET LEADERBOARD (top {topCount}) ===");
        var totalUsers = _context.Users.Count();
        Console.WriteLine($"Total users in DB: {totalUsers}");
        // Sort using CompareTo (which handles the ranking logic)
        var sortedUsers = _context.Users.AsEnumerable().OrderBy(u => u).ToList();
        // Debug output
        Console.WriteLine("Leaderboard:");
        int rank = 1;
        foreach (var user in sortedUsers.Take(topCount)) {
            Console.WriteLine($"  {rank}. {user.Username} - Games: {user.TotalGamesPlayed}, Avg: {user.AverageCompatibilityScore:F2}, Best: {user.BestMatchesCount}");
            rank++;
        }
        return sortedUsers.Take(topCount).ToList();
    }
    
    /// <summary>
    /// Retrieves the rank position of a specific user within the leaderboard.
    /// </summary>
    /// <param name="username">The username whose rank should be retrieved.</param>
    /// <returns>The user's rank position (1-based index), or 0 if the user is not found.</returns>
    public int GetUserRank(string username) {
        Console.WriteLine($"\n=== GET USER RANK for '{username}' ===");
        var sortedUsers = _context.Users.AsEnumerable().OrderBy(u => u).ToList();
        // Čia galime naudoti StringComparison, nes jau ne LINQ užklausa
        var rank = sortedUsers.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
        Console.WriteLine($"Rank for {username}: {rank} out of {sortedUsers.Count}");
        return rank;
    }
    
    /// <summary>
    /// Retrieves a user record by username.
    /// </summary>
    /// <param name="username">The username of the user to find.</param>
    /// <returns>
    /// The matching <see cref="User"/> object if found; otherwise <c>null</c>.
    /// </returns>
    public User? GetUserByUsername(string username) {
        Console.WriteLine($"GetUserByUsername: '{username}'");
        // PATAISYTA: naudojame ToLower()
        var usernameLower = username.ToLower();
        var user = _context.Users
            .Where(u => u.Username.ToLower() == usernameLower)
            .FirstOrDefault();
        Console.WriteLine($"User found: {user != null}");
        return user;
    }
}

