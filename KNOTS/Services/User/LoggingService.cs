namespace KNOTS.Services;

public class LoggingService {
    private readonly string _logDirectory;
    private readonly string _errorLogPath;
    
    public LoggingService(string logDirectory = "logs") {
        _logDirectory = logDirectory;
        _errorLogPath = Path.Combine(_logDirectory, "errors.log");
        Directory.CreateDirectory(_logDirectory);
    }
    public void LogException(Exception ex, string context) {
        try {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context} - {ex.Message}\n{ex.StackTrace}\n;";
            File.AppendAllText(_errorLogPath, logEntry);
        }
        catch { Console.WriteLine($"Logging failed");}
    }
}