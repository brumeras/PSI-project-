namespace KNOTS.Services;

/// <summary>
/// Simple internal logger for console output.
/// Provides methods to log informational and error messages with timestamps.
/// </summary>
internal static class Logger
{
    /// <summary>
    /// Logs an informational message to the console with a timestamp.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Info(string message) { Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");}
    
    /// <summary>
    /// Logs an error message and exception to the console.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="ex">The exception associated with the error.</param>
    public static void Error(string message, Exception ex) { Console.WriteLine($"[ERROR] {message}: {ex.Message}"); }
}