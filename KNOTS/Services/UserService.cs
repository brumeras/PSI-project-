using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KNOTS.Services
{
    public class User : IEquatable<User>
    {
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> Friends { get; set; } = new List<string>();
        public string PasswordHash { get; set; } = string.Empty;

        public bool Equals(User? other) {
            if(other is null) return false;
            return Username.Equals(other.Username, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public override bool Equals(object? obj) => Equals(obj as User);
        public override int GetHashCode() => Username.ToLowerInvariant().GetHashCode();
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

        // Load users from file using stream
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
                        }
                        else
                        {
                            _users = new List<User>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading users file: {ex.Message}");
                _users = new List<User>();
            }
        }

        // Save users to file using stream
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users file: {ex.Message}");
            }
        }

        // Register new user
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

            // Check if username is already taken
            if (_users.Contains(new User { Username = username}))
            {
                return (false, "This username is already taken.");
            }

            // Create new user
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

        // Login user
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

        // Logout user
        public void LogoutUser()
        {
            CurrentUser = null;
            OnAuthenticationChanged?.Invoke();
        }

        // Add friend
        public (bool Success, string Message) AddFriend(string friendUsername)
        {
            if (!IsAuthenticated)
            {
                return (false, "You must be logged in.");
            }

            if (string.IsNullOrWhiteSpace(friendUsername))
            {
                return (false, "Friend username cannot be empty.");
            }

            if (friendUsername.Equals(CurrentUser, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "You cannot add yourself as a friend.");
            }

            // Check if friend exists
            var friendExists = _users.Any(u => u.Username.Equals(friendUsername, StringComparison.OrdinalIgnoreCase));
            if (!friendExists)
            {
                return (false, "User with this username does not exist.");
            }

            var currentUserData = _users.FirstOrDefault(u => u.Username.Equals(CurrentUser, StringComparison.OrdinalIgnoreCase));
            if (currentUserData != null)
            {
                // Check if already a friend
                if (currentUserData.Friends.Any(f => f.Equals(friendUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "This user is already in your friends list.");
                }

                currentUserData.Friends.Add(friendUsername);
                SaveUsers();
                return (true, $"Friend {friendUsername} added successfully!");
            }

            return (false, "Error adding friend.");
        }
        

        // Get total users count (statistics)
        public int GetTotalUsersCount()
        {
            return _users.Count;
        }
    }
}