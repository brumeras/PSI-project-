namespace KNOTS.Services.Interfaces;

public interface InterfaceLoggingService
{
    void LogException(Exception ex, string context);
}