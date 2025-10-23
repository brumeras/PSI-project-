using System.Text.Json;

namespace KNOTS.Services;

/// <summary>
/// Provides generic file-based persistence using JSON serialization for any type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object to store/load. Must have a parameterless constructor.</typeparam>
public class JsonFileRepository<T> where T : new() {
    private readonly string _filePath;
    
    /// <summary>
    /// Initializes a new instance of <see cref="JsonFileRepository{T}"/> with the specified directory and file name.
    /// Ensures that the directory exists.
    /// </summary>
    /// <param name="directory">The directory to store the file.</param>
    /// <param name="fileName">The file name to store the serialized data.</param>
    public JsonFileRepository(string directory, string fileName) {
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, fileName);
    }
    
    /// <summary>
    /// Loads the object from the JSON file.
    /// </summary>
    /// <returns>An instance of <typeparamref name="T"/>. Returns a new instance if the file does not exist or deserialization fails.</returns>
    public T Load() {
        try {
            if (File.Exists(_filePath)) {
                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(fs);
                string json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading from {_filePath}: {ex.Message}"); }
        return new T();
    }

    
    /// <summary>
    /// Saves the object to the JSON file.
    /// </summary>
    /// <param name="data">The object of type <typeparamref name="T"/> to serialize and save.</param>
    public void Save(T data) {
        try {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions {
                WriteIndented = true
            });
            using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fs);
            writer.Write(json);
        }
        catch (Exception ex) { Console.WriteLine($"Error saving to {_filePath}: {ex.Message}"); }
    }
}