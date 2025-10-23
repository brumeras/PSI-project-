using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KNOTS.Services
{
    // ===== MAIN SERVICE  =====
    
    /// <summary>
    /// Manages users in the system, including authentication, statistics, and leaderboard.
    /// </summary>
    public class UserService
    {
        private readonly UserFileStorage _storage;
        private List<User> _users = new();
        
        /// <summary>
        /// Event triggered whenever the authentication state changes (login/logout).
        /// </summary>
        public event Action? OnAuthenticationChanged;

        /// <summary>Username of the currently authenticated user, or <c>null</c> if no user is logged in.</summary>
        public string? CurrentUser { get; private set; }
        
        /// <summary>Indicates whether a user is currently logged in.</summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);
        
        
        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/> and loads users from storage.
        /// </summary>
        public UserService() {
            _storage = new UserFileStorage();
            _users = _storage.LoadUsers();
        }

        // ===== AUTHENTICATION =====
        
        /// <summary>
        /// Registers a new user with a username and password.
        /// </summary>
        /// <param name="username">The desired username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A tuple containing success status and a message.</returns>
        public (bool Success, string Message) RegisterUser(string username, string password) {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return (false, "Username and password cannot be empty.");

            if (username.Length < 3) return (false, "Username must be at least 3 characters long.");

            if (password.Length < 4) return (false, "Password must be at least 4 characters long.");

            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return (false, "This username is already taken.");

            var newUser = new User {
                Username = username,
                PasswordHash = PasswordHasher.Hash(password),
                CreatedAt = DateTime.Now
            };

            _users.Add(newUser);
            SaveUsers();
            Logger.Info($"User registered: {username}");
            return (true, "Registration successful!");
        }

        /// <summary>
        /// Authenticates a user with a username and password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A tuple containing success status and a message.</returns>
        public (bool Success, string Message) LoginUser(string username, string password) {
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash)) return (false, "Invalid username or password.");

            CurrentUser = user.Username;
            OnAuthenticationChanged?.Invoke();
            Logger.Info($"User logged in: {username}");
            return (true, "Login successful!");
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        public void LogoutUser() {
            Logger.Info($"User logged out: {CurrentUser}");
            CurrentUser = null;
            OnAuthenticationChanged?.Invoke();
        }

        // ===== STATISTICS =====
        
        /// <summary>
        /// Updates the statistics for a user after a game.
        /// </summary>
        /// <param name="username">The username of the player.</param>
        /// <param name="compatibilityScore">The score from the game.</param>
        /// <param name="wasBestMatch">Whether the player was the best match in the game.</param>
        public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch){
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
        
        /// <summary>
        /// Gets the top users in the leaderboard, sorted by ranking criteria.
        /// </summary>
        /// <param name="topCount">The maximum number of users to return.</param>
        /// <returns>A list of <see cref="User"/> objects.</returns>
        public List<User> GetLeaderboard(int topCount = 10) {
            ReloadUsers();
            var sorted = _users.OrderBy(u => u).Take(topCount).ToList();
            Logger.Info($"Leaderboard generated. Total users: {_users.Count}");
            return sorted;
        }

        /// <summary>
        /// Gets the ranking of a specific user.
        /// </summary>
        /// <param name="username">The username of the player.</param>
        /// <returns>The rank (1-based) of the user, or -1 if not found.</returns>
        public int GetUserRank(string username) {
            ReloadUsers();
            var sorted = _users.OrderBy(u => u).ToList();
            int rank = sorted.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
            Logger.Info($"Rank for {username}: {rank}");
            return rank;
        }

        /// <summary>Finds a user by username.</summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The <see cref="User"/> object, or <c>null</c> if not found.</returns>
        public User? GetUserByUsername(string username) { return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)); }
        
        /// <summary>Returns the total number of registered users.</summary>
        public int GetTotalUsersCount() {
            ReloadUsers();
            return _users.Count;
        }
        
        // ===== PRIVATE HELPERS =====
        
        private void ReloadUsers() => _users = _storage.LoadUsers();
        private void SaveUsers() => _storage.SaveUsers(_users);
    }
}
