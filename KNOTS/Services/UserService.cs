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
        
        public int TotalGamesPlayed { get; set; } = 0;
        public double AverageCompatibilityScore { get; set; } = 0.0;
        public int BestMatchesCount { get; set; } = 0;
        
        public int CompareTo(User? other)
        {
            if (other == null) return 1;
            
            // Higher BestMatchesCount = better (other.BestMatchesCount compared to this)
            int bestMatchComparison = other.BestMatchesCount.CompareTo(this.BestMatchesCount);
            if (bestMatchComparison != 0) return bestMatchComparison;
            
            // Higher average score = better
            int avgScoreComparison = other.AverageCompatibilityScore.CompareTo(this.AverageCompatibilityScore);
            if (avgScoreComparison != 0) return avgScoreComparison;
            
            // More games = better
            int gamesComparison = other.TotalGamesPlayed.CompareTo(this.TotalGamesPlayed);
            if (gamesComparison != 0) return gamesComparison;
            
            // Alphabetical as last resort
            return string.Compare(this.Username, other.Username, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool Equals(User? other)
        {
            if (other == null) return false;
            return this.Username.Equals(other.Username, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as User);
        }

        public override int GetHashCode()
        {
            return Username.ToLowerInvariant().GetHashCode();
        }

        public static bool operator ==(User? left, User? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(User? left, User? right)
        {
            return !(left == right);
        }

        public static bool operator <(User? left, User? right)
        {
            if (left is null) return right is not null;
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(User? left, User? right)
        {
            if (left is null) return false;
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(User? left, User? right)
        {
            if (left is null) return true;
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(User? left, User? right)
        {
            if (left is null) return right is null;
            return left.CompareTo(right) >= 0;
        }
    }

    public class UserService
    {
        private readonly string _filePath = "users.json";
        private List<User> _users = new List<User>();

        public UserService()
        {
            LoadUsers();
        }

        public event Action? OnAuthenticationChanged;

        public string? CurrentUser { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (fileStream.Length > 0)
                        {
                            _users = JsonSerializer.Deserialize<List<User>>(fileStream) ?? new List<User>();
                            Console.WriteLine($"Loaded {_users.Count} users from file");
                        }
                        else
                        {
                            _users = new List<User>();
                            Console.WriteLine("Users file is empty");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Users file does not exist yet");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading users file: {ex.Message}");
                _users = new List<User>();
            }
        }
        
        private void SaveUsers()
        {
            try
            {
                using (FileStream fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    JsonSerializer.Serialize(fileStream, _users, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }
                Console.WriteLine($"Saved {_users.Count} users to file");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users file: {ex.Message}");
            }
        }
        
        public (bool Success, string Message) RegisterUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password cannot be empty.");
            }

            if (username.Length < 3)
            {
                return (false, "Username must be at least 3 characters long.");
            }

            if (password.Length < 4)
            {
                return (false, "Password must be at least 4 characters long.");
            }
            
            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "This username is already taken.");
            }
            
            var newUser = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13),
                CreatedAt = DateTime.Now
            };

            _users.Add(newUser);
            SaveUsers();

            return (true, "Registration successful! You can now log in.");
        }
        
        public (bool Success, string Message) LoginUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password cannot be empty.");
            }

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                if (BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash))
                {
                    CurrentUser = user.Username;
                    OnAuthenticationChanged?.Invoke();
                    return (true, "Login successful!");
                }
            }

            return (false, "Invalid username or password.");
        }
        
        public void LogoutUser()
        {
            CurrentUser = null;
            OnAuthenticationChanged?.Invoke();
        }

        public int GetTotalUsersCount()
        {
            LoadUsers(); // Reload to get latest count
            return _users.Count;
        }

        public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch)
        {
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    
            if (user != null)
            {
                user.TotalGamesPlayed++;
        
                if (wasBestMatch)
                {
                    user.BestMatchesCount++;
                }
                
                user.AverageCompatibilityScore = 
                    ((user.AverageCompatibilityScore * (user.TotalGamesPlayed - 1)) + compatibilityScore) 
                    / user.TotalGamesPlayed;
        
                SaveUsers();
        
                Console.WriteLine($"Updated stats for {username}: Games={user.TotalGamesPlayed}, AvgScore={user.AverageCompatibilityScore:F2}, BestMatches={user.BestMatchesCount}");
            }
        }

        public List<User> GetLeaderboard(int topCount = 10)
        {
            LoadUsers(); // Reload to get latest data
            
            Console.WriteLine($"GetLeaderboard called. Total users: {_users.Count}");
            
            // Sort using CompareTo (which handles the ranking logic)
            var sortedUsers = _users.OrderBy(u => u).ToList();
            
            // Debug output
            foreach (var user in sortedUsers.Take(topCount))
            {
                Console.WriteLine($"User: {user.Username}, Games: {user.TotalGamesPlayed}, Avg: {user.AverageCompatibilityScore:F2}, Best: {user.BestMatchesCount}");
            }
            
            return sortedUsers.Take(topCount).ToList();
        }

        public int GetUserRank(string username)
        {
            LoadUsers(); // Reload to get latest data
            var sortedUsers = _users.OrderBy(u => u).ToList();
            var rank = sortedUsers.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) + 1;
            Console.WriteLine($"Rank for {username}: {rank}");
            return rank;
        }

        public User? GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }
}