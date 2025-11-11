namespace KNOTS.Services;

public interface InterfaceUserService
{
    string? CurrentUser { get; }
    bool IsAuthenticated { get; }
    event Action? OnAuthenticationChanged;
    
    (bool Success, string Message) RegisterUser(string username, string password);
    (bool Success, string Message) LoginUser(string username, string password);
    void LogoutUser();
    int GetTotalUsersCount();
    void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch);
    List<User> GetLeaderboard(int topCount = 10);
    int GetUserRank(string username);
}