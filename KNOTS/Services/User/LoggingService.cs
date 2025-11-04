namespace KNOTS.Services;

/// <summary>
/// Provides centralized logging functionality for the application.
/// </summary>
public class LoggingService
{
    private readonly string _logDirectory;
    private readonly string _errorLogPath;
    
    public LoggingService(string logDirectory = "logs")
    {
        _logDirectory = logDirectory;
        _errorLogPath = Path.Combine(_logDirectory, "errors.log");
        
        // Ensure log directory exists
        Directory.CreateDirectory(_logDirectory);
    }
    
    /// <summary>
    /// Logs an exception to a file with context information.
    /// </summary>
    public void LogException(Exception ex, string context)
    {
        try 
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n" +
                             $"Exception Type: {ex.GetType().Name}\n" +
                             $"Message: {ex.Message}\n" +
                             $"Stack Trace: {ex.StackTrace}\n" +
                             $"{new string('-', 80)}\n\n";
                             
            File.AppendAllText(_errorLogPath, logEntry);
            Console.WriteLine($"✅ Exception logged to {_errorLogPath}");
        }
        catch (Exception logEx)
        {
            // Fallback to console if file logging fails
            Console.WriteLine($"❌ Failed to log exception to file: {logEx.Message}");
            Console.WriteLine($"Original exception: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Logs general information messages.
    /// </summary>
    public void LogInfo(string message, string context = "INFO")
    {
        try
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{context}] {message}\n";
            string infoLogPath = Path.Combine(_logDirectory, "info.log");
            File.AppendAllText(infoLogPath, logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to log info: {ex.Message}");
        }
    }
}