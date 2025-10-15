using System.Text.Json;

namespace KNOTS.Services;

internal class UserFileStorage
{
    private readonly string _filePath;

    public UserFileStorage(string filePath = "users.json") { _filePath = filePath; }

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
    public void SaveUsers(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}