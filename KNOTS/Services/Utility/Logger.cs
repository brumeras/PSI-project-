namespace KNOTS.Services;
    internal static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static void Error(string message, Exception ex)
        {
            Console.WriteLine($"[ERROR {DateTime.Now:HH:mm:ss}] {message}: {ex.Message}");
        }
    }
