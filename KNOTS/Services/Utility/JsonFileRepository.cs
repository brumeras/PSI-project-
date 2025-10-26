using System;
using System.IO;
using System.Text.Json;

namespace KNOTS.Services;

public class JsonFileRepository<T> where T : new() {
    private readonly string _filePath;
    public JsonFileRepository(string directory, string fileName) {
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, fileName);
    }
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