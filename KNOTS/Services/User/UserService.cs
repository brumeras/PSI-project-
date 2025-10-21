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

namespace KNOTS.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
            Logger.Info("UserService created");
        }

        public event Action? OnAuthenticationChanged;
        public string? CurrentUser { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);

        // ===== REGISTRATION =====
        public (bool Success, string Message) RegisterUser(string username, string password)
        {
            Logger.Info($"Registration attempt for '{username}'");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password cannot be empty.");

            if (username.Length < 3)
                return (false, "Username must be at least 3 characters long.");

            if (password.Length < 4)
                return (false, "Password must be at least 4 characters long.");

            try
            {
                string usernameLower = username.ToLower();
                var existingUser = _context.Users
                    .FirstOrDefault(u => u.Username.ToLower() == usernameLower);

                if (existingUser != null)
                    return (false, "This username is already taken.");

                var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                Logger.Info($"✅ User registered: {username}");
                return (true, "Registration successful!");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during registration", ex);
                return (false, "Registration error.");
            }
        }

        // ===== LOGIN =====
        public (bool Success, string Message) LoginUser(string username, string password)
        {
            Logger.Info($"Login attempt: {username}");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password cannot be empty.");

            try
            {
                string usernameLower = username.ToLower();
                var user = _context.Users
                    .FirstOrDefault(u => u.Username.ToLower() == usernameLower);

                if (user == null)
                    return (false, "Invalid username or password.");

                bool passwordValid = BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash);

                if (!passwordValid)
                    return (false, "Invalid username or password.");

                CurrentUser = user.Username;
                OnAuthenticationChanged?.Invoke();

                Logger.Info($"✅ Login successful: {username}");
                return (true, "Login successful!");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during login", ex);
                return (false, "Login error.");
            }
        }

        // ===== LOGOUT =====
        public void LogoutUser()
        {
            if (CurrentUser != null)
                Logger.Info($"User logged out: {CurrentUser}");
            CurrentUser = null;
            OnAuthenticationChanged?.Invoke();
        }

        // ===== STATS =====
        public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch)
        {
            var usernameLower = username.ToLower();
            var user = _context.Users
                .FirstOrDefault(u => u.Username.ToLower() == usernameLower);

            if (user == null)
            {
                Logger.Info($"⚠️ User '{username}' not found for stats update");
                return;
            }

            user.TotalGamesPlayed++;
            if (wasBestMatch) user.BestMatchesCount++;

            user.AverageCompatibilityScore =
                ((user.AverageCompatibilityScore * (user.TotalGamesPlayed - 1)) + compatibilityScore) /
                user.TotalGamesPlayed;

            _context.SaveChanges();
            Logger.Info($"Stats updated for {username}: Games={user.TotalGamesPlayed}, Avg={user.AverageCompatibilityScore:F2}, Best={user.BestMatchesCount}");
        }

        // ===== LEADERBOARD =====
        public List<User> GetLeaderboard(int topCount = 10)
        {
            var sorted = _context.Users.AsEnumerable().OrderBy(u => u).Take(topCount).ToList();
            Logger.Info($"Leaderboard generated. Total users: {_context.Users.Count()}");
            return sorted;
        }

        public int GetUserRank(string username)
        {
            var sorted = _context.Users.AsEnumerable().OrderBy(u => u).ToList();
            int rank = sorted.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
            Logger.Info($"Rank for {username}: {rank}");
            return rank;
        }

        public User? GetUserByUsername(string username)
        {
            return _context.Users
                .FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        }

        public int GetTotalUsersCount() => _context.Users.Count();
    }
}
