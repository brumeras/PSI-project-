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
public class UserService {
    private readonly AppDbContext _context;
    public UserService(AppDbContext context) {
        _context = context;
        Console.WriteLine("🔧 UserService created");
    }
    public event Action? OnAuthenticationChanged;
    public string? CurrentUser { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);
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
    public void LogoutUser() {
        Console.WriteLine($"\n=== LOGOUT ===");
        Console.WriteLine($"Logging out user: {CurrentUser}");
        CurrentUser = null;
        OnAuthenticationChanged?.Invoke();
        Console.WriteLine("✅ Logout successful");
    }
    public int GetTotalUsersCount() {
        var count = _context.Users.Count();
        Console.WriteLine($"GetTotalUsersCount: {count}");
        return count;
    }
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
    public int GetUserRank(string username) {
        Console.WriteLine($"\n=== GET USER RANK for '{username}' ===");
        var sortedUsers = _context.Users.AsEnumerable().OrderBy(u => u).ToList();
        // Čia galime naudoti StringComparison, nes jau ne LINQ užklausa
        var rank = sortedUsers.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
        Console.WriteLine($"Rank for {username}: {rank} out of {sortedUsers.Count}");
        return rank;
    }
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

