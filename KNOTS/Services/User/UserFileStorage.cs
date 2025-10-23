using System.Text.Json;

namespace KNOTS.Services;

/// <summary>
/// Provides file-based persistence for <see cref="User"/> objects using JSON serialization.
/// </summary>
/// <remarks>
/// This class handles loading and saving users to a local JSON file.
/// It is marked as internal and intended for use within the user management services.
/// </remarks>
internal class UserFileStorage
{
    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of <see cref="UserFileStorage"/>.
    /// </summary>
    /// <param name="filePath">Path to the JSON file. Defaults to "users.json".</param>
    public UserFileStorage(string filePath = "users.json") { _filePath = filePath; }

    
    /// <summary>
    /// Loads the list of users from the JSON file.
    /// </summary>
    /// <returns>A list of <see cref="User"/> objects. Returns an empty list if the file does not exist or deserialization fails.</returns>
    public List<User> LoadUsers() {
        if (!File.Exists(_filePath)) return new List<User>();
        try {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        catch {
            return new List<User>();
        }
    }
    
    /// <summary>
    /// Saves the list of users to the JSON file.
    /// </summary>
    /// <param name="users">The list of <see cref="User"/> objects to save.</param>
    public void SaveUsers(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}