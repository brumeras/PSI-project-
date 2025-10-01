using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace KNOTS.Services
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> Friends { get; set; } = new List<string>();
    }

    // Shared user data storage (singleton)
    public class UserDataStore
    {
        private readonly string _filePath = "users.json";
        private List<User> _users = new List<User>();
        private readonly object _lock = new object();

        public UserDataStore()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string jsonString = File.ReadAllText(_filePath);
                    _users = JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();
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
                string jsonString = JsonSerializer.Serialize(_users, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users file: {ex.Message}");
            }
        }

        public (bool Success, string Message) RegisterUser(string username, string password)
        {
            lock (_lock)
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
                    Password = password,
                    CreatedAt = DateTime.Now
                };

                _users.Add(newUser);
                SaveUsers();

                return (true, "Registration successful! You can now log in.");
            }
        }

        public User? ValidateUser(string username, string password)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == password);
            }
        }

        public bool UserExists(string username)
        {
            lock (_lock)
            {
                return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        public (bool Success, string Message) AddFriend(string username, string friendUsername)
        {
            lock (_lock)
            {
                if (friendUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "You cannot add yourself as a friend.");
                }

                if (!_users.Any(u => u.Username.Equals(friendUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "User with this username does not exist.");
                }

                var currentUserData = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (currentUserData != null)
                {
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
        }

        public (bool Success, string Message) RemoveFriend(string username, string friendUsername)
        {
            lock (_lock)
            {
                var currentUserData = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (currentUserData != null)
                {
                    var removed = currentUserData.Friends.RemoveAll(f => f.Equals(friendUsername, StringComparison.OrdinalIgnoreCase));
                    if (removed > 0)
                    {
                        SaveUsers();
                        return (true, $"Friend {friendUsername} removed successfully!");
                    }
                    return (false, "Friend not found in your friends list.");
                }

                return (false, "Error removing friend.");
            }
        }

        public List<string> GetUserFriends(string username)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                return user?.Friends ?? new List<string>();
            }
        }

        public int GetTotalUsersCount()
        {
            lock (_lock)
            {
                return _users.Count;
            }
        }
    }

    // Circuit handler that stores username in the circuit itself
    public class AuthenticationCircuitHandler : CircuitHandler
    {
        private static readonly ConcurrentDictionary<string, string> _circuitUsers = new();

        public string? CircuitId { get; private set; }

        public string? Username
        {
            get => CircuitId != null && _circuitUsers.TryGetValue(CircuitId, out var user) ? user : null;
            set
            {
                if (CircuitId != null)
                {
                    if (value != null)
                    {
                        _circuitUsers[CircuitId] = value;
                    }
                    else
                    {
                        _circuitUsers.TryRemove(CircuitId, out _);
                    }
                }
            }
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            CircuitId = circuit.Id;
            Console.WriteLine($"Circuit opened: {circuit.Id}");
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _circuitUsers.TryRemove(circuit.Id, out var username);
            Console.WriteLine($"Circuit closed: {circuit.Id}, User: {username}");
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }

    // User service - SCOPED (one per circuit)
    public class UserService
    {
        private readonly UserDataStore _userDataStore;
        private readonly AuthenticationCircuitHandler _circuitHandler;

        public UserService(UserDataStore userDataStore, AuthenticationCircuitHandler circuitHandler)
        {
            _userDataStore = userDataStore;
            _circuitHandler = circuitHandler;
        }

        public event Action? OnAuthenticationChanged;

        public string? CurrentUser => _circuitHandler.Username;

        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser);

        public (bool Success, string Message) RegisterUser(string username, string password)
        {
            return _userDataStore.RegisterUser(username, password);
        }

        public (bool Success, string Message) LoginUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password cannot be empty.");
            }

            var user = _userDataStore.ValidateUser(username, password);

            if (user != null)
            {
                _circuitHandler.Username = user.Username;
                OnAuthenticationChanged?.Invoke();
                Console.WriteLine($"User {user.Username} logged in on circuit {_circuitHandler.CircuitId}");
                return (true, "Login successful!");
            }

            return (false, "Invalid username or password.");
        }

        public void LogoutUser()
        {
            var username = CurrentUser;
            _circuitHandler.Username = null;
            Console.WriteLine($"User {username} logged out from circuit {_circuitHandler.CircuitId}");
            OnAuthenticationChanged?.Invoke();
        }

        public (bool Success, string Message) AddFriend(string friendUsername)
        {
            if (!IsAuthenticated || CurrentUser == null)
            {
                return (false, "You must be logged in.");
            }

            if (string.IsNullOrWhiteSpace(friendUsername))
            {
                return (false, "Friend username cannot be empty.");
            }

            return _userDataStore.AddFriend(CurrentUser, friendUsername);
        }

        public (bool Success, string Message) RemoveFriend(string friendUsername)
        {
            if (!IsAuthenticated || CurrentUser == null)
            {
                return (false, "You must be logged in.");
            }

            return _userDataStore.RemoveFriend(CurrentUser, friendUsername);
        }

        public List<string> GetUserFriends()
        {
            if (!IsAuthenticated || CurrentUser == null)
                return new List<string>();

            return _userDataStore.GetUserFriends(CurrentUser);
        }

        public int GetTotalUsersCount()
        {
            return _userDataStore.GetTotalUsersCount();
        }

        public bool UserExists(string username)
        {
            return _userDataStore.UserExists(username);
        }
    }
}