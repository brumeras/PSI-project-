using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KNOTS.Services
{
    public class User : IComparable<User>, IEquatable<User>
    {
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string PasswordHash { get; set; } = string.Empty;

        public int TotalGamesPlayed { get; set; }
        public double AverageCompatibilityScore { get; set; }
        public int BestMatchesCount { get; set; }

        public int CompareTo(User? other)
        {
            if (other == null) return 1;

            int bestMatchComparison = other.BestMatchesCount.CompareTo(this.BestMatchesCount);
            if (bestMatchComparison != 0) return bestMatchComparison;

            int avgScoreComparison = other.AverageCompatibilityScore.CompareTo(this.AverageCompatibilityScore);
            if (avgScoreComparison != 0) return avgScoreComparison;

            int gamesComparison = other.TotalGamesPlayed.CompareTo(this.TotalGamesPlayed);
            if (gamesComparison != 0) return gamesComparison;

            return string.Compare(this.Username, other.Username, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(User? other) =>
            other != null && Username.Equals(other.Username, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as User);
        public override int GetHashCode() => Username.ToLowerInvariant().GetHashCode();

        public static bool operator ==(User? left, User? right) => Equals(left, right);
        public static bool operator !=(User? left, User? right) => !Equals(left, right);
    }

    // ===== HELPERS  =====

    internal static class PasswordHasher
    {
        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);

        public static bool Verify(string password, string hash) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }

    internal class UserFileStorage
    {
        private readonly string _filePath;
        public UserFileStorage(string filePath = "users.json")
        {
            _filePath = filePath;
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(_filePath)) return new List<User>();
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    internal static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static void Error(string message, Exception ex)
        {
            Console.WriteLine($"[ERROR] {message}: {ex.Message}");
        }
    }
    
    // ===== MAIN SERVICE  =====
    public class UserService
    {
        private readonly UserFileStorage _storage;
        private List<User> _users = new();
        public event Action? OnAuthenticationChanged;

        public string? CurrentUser { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);

        public UserService()
        {
            _storage = new UserFileStorage();
            _users = _storage.LoadUsers();
        }

        // ===== AUTHENTICATION =====
        public (bool Success, string Message) RegisterUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password cannot be empty.");

            if (username.Length < 3)
                return (false, "Username must be at least 3 characters long.");

            if (password.Length < 4)
                return (false, "Password must be at least 4 characters long.");

            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return (false, "This username is already taken.");

            var newUser = new User
            {
                Username = username,
                PasswordHash = PasswordHasher.Hash(password),
                CreatedAt = DateTime.Now
            };

            _users.Add(newUser);
            SaveUsers();
            Logger.Info($"User registered: {username}");
            return (true, "Registration successful!");
        }

        public (bool Success, string Message) LoginUser(string username, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                return (false, "Invalid username or password.");

            CurrentUser = user.Username;
            OnAuthenticationChanged?.Invoke();
            Logger.Info($"User logged in: {username}");
            return (true, "Login successful!");
        }

        public void LogoutUser()
        {
            Logger.Info($"User logged out: {CurrentUser}");
            CurrentUser = null;
            OnAuthenticationChanged?.Invoke();
        }

        // ===== STATISTICS =====
        public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch)
        {
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null) return;

            user.TotalGamesPlayed++;
            if (wasBestMatch) user.BestMatchesCount++;
            user.AverageCompatibilityScore =
                ((user.AverageCompatibilityScore * (user.TotalGamesPlayed - 1)) + compatibilityScore)
                / user.TotalGamesPlayed;

            SaveUsers();
            Logger.Info($"Stats updated for {username}: Games={user.TotalGamesPlayed}, Avg={user.AverageCompatibilityScore:F2}, Best={user.BestMatchesCount}");
        }

        // ===== LEADERBOARD =====
        public List<User> GetLeaderboard(int topCount = 10)
        {
            ReloadUsers();
            var sorted = _users.OrderBy(u => u).Take(topCount).ToList();
            Logger.Info($"Leaderboard generated. Total users: {_users.Count}");
            return sorted;
        }

        public int GetUserRank(string username)
        {
            ReloadUsers();
            var sorted = _users.OrderBy(u => u).ToList();
            int rank = sorted.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
            Logger.Info($"Rank for {username}: {rank}");
            return rank;
        }

        public User? GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        public int GetTotalUsersCount()
        {
            ReloadUsers();
            return _users.Count;
        }
      
        private void ReloadUsers() => _users = _storage.LoadUsers();
        private void SaveUsers() => _storage.SaveUsers(_users);
    }
}
